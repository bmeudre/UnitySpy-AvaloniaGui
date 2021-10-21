#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <mach-o/dyld_images.h>
#include <mach-o/loader.h>
#include <mach/vm_map.h>

#define PATH_MAX 2048

int str_ends_with(const char *str, const char *suffix)
{
    if (!str || !suffix)
        return 0;
    size_t lenstr = strlen(str);
    size_t lensuffix = strlen(suffix);
    if (lensuffix >  lenstr)
        return 0;
    return strncmp(str + lenstr - lensuffix, suffix, lensuffix) == 0;
}

size_t size_of_image(struct mach_header_64 *header) {
    size_t sz = sizeof(*header); // Size of the header
    sz += header->sizeofcmds;    // Size of the load commands

    struct load_command *lc = (struct load_command *) (header + 1);
    for (uint32_t i = 0; i < header->ncmds; i++) {
        if (lc->cmd == LC_SEGMENT_64) {
            sz += ((struct segment_command_64 *) lc)->vmsize; // Size of segments
        }
        lc = (struct load_command *) ((char *) lc + lc->cmdsize);
    }
    return sz;
}

// Helper function to read process memory (a la Win32 API of same name) To make
// it easier for inclusion elsewhere, it takes a pid, and does the task_for_pid
// by itself. Given that iOS invalidates task ports after use, it's actually a
// good idea, since we'd need to reget anyway
unsigned char* read_process_memory (int pid, mach_vm_address_t addr, mach_msg_type_number_t* size)
{
    task_t t;
    task_for_pid(mach_task_self(), pid, &t);
    mach_msg_type_number_t dataCnt = (mach_msg_type_number_t) *size;
    vm_offset_t readMem;

    // Use vm_read, rather than mach_vm_read, since the latter is different in
    // iOS.

        kern_return_t kr = vm_read(t,           // vm_map_t target_task,
                     addr,                      // mach_vm_address_t address,
                     *size,                     // mach_vm_size_t size
                     &readMem,                  //vm_offset_t *data,
                     &dataCnt);                 // mach_msg_type_number_t *dataCnt

        if (kr) 
        {
            return NULL;
        }

    return (unsigned char*)readMem;
}

int read_process_memory_to_buffer(int pid, mach_vm_address_t addr, char* buffer, unsigned int size) {
    char* result = read_process_memory(pid, addr, &size);
    for(int i = 0; i < size; i++) {
        buffer[i] = result[i];
    }
    return result == NULL;
}

char* get_module_info(int pid, const char* module_name, int* module_size) {
    task_t task;
    task_for_pid(mach_task_self(), pid, &task);

    struct task_dyld_info dyld_info;
    mach_msg_type_number_t count = TASK_DYLD_INFO_COUNT;

    if (task_info(task, TASK_DYLD_INFO, (task_info_t) &dyld_info, &count)
            == KERN_SUCCESS) {
        mach_msg_type_number_t size = sizeof(struct dyld_all_image_infos);

        uint8_t* data =
            read_process_memory(pid, dyld_info.all_image_info_addr, &size);
        struct dyld_all_image_infos* infos = (struct dyld_all_image_infos *) data;

        mach_msg_type_number_t size2 =
            sizeof(struct dyld_image_info) * infos->infoArrayCount;
        uint8_t* info_addr =
            read_process_memory(pid, (mach_vm_address_t) infos->infoArray, &size2);
        struct dyld_image_info* info = (struct dyld_image_info*) info_addr;

        for (int i=0; i < infos->infoArrayCount; i++)
        {
            mach_msg_type_number_t size3 = PATH_MAX;

            char* fpath_addr = read_process_memory(pid, (mach_vm_address_t) info[i].imageFilePath, &size3);            

            if (str_ends_with(fpath_addr, module_name))
            {
                struct mach_header_64* header;

                int size4 = sizeof(struct mach_header_64);
                header = read_process_memory(pid, (mach_vm_address_t) info[i].imageLoadAddress, &size4);

                size4 = sizeof(struct mach_header_64) + header->sizeofcmds;
                header = read_process_memory(pid, (mach_vm_address_t) info[i].imageLoadAddress, &size4);

                (*module_size) = size_of_image(header);
                return info[i].imageLoadAddress;
            }
        }
    }
    return 0;
}
