#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <mach-o/dyld_images.h>
#include <mach/vm_map.h>

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

// Helper function to read process memory (a la Win32 API of same name) To make
// it easier for inclusion elsewhere, it takes a pid, and does the task_for_pid
// by itself. Given that iOS invalidates task ports after use, it's actually a
// good idea, since we'd need to reget anyway
unsigned char* readProcessMemory (int pid, mach_vm_address_t addr, mach_msg_type_number_t* size)
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

unsigned long* get_module_info(int pid, const char* moduleName, int* moduleSize) {

    task_t task;
    task_for_pid(mach_task_self(), pid, &task);

    struct task_dyld_info dyld_info;
    mach_msg_type_number_t count = TASK_DYLD_INFO_COUNT;

    if (task_info(task, TASK_DYLD_INFO, (task_info_t) &dyld_info, &count)
            == KERN_SUCCESS) {
        mach_msg_type_number_t size = sizeof(struct dyld_all_image_infos);

        uint8_t* data =
            readProcessMemory(pid, dyld_info.all_image_info_addr, &size);
        struct dyld_all_image_infos* infos = (struct dyld_all_image_infos *) data;

        mach_msg_type_number_t size2 =
            sizeof(struct dyld_image_info) * infos->infoArrayCount;
        uint8_t* info_addr =
            readProcessMemory(pid, (mach_vm_address_t) infos->infoArray, &size2);
        struct dyld_image_info* info = (struct dyld_image_info*) info_addr;

        for (int i=0; i < infos->infoArrayCount; i++) {

            if (str_ends_with(info[i].imageFilePath, moduleName))
            {
                (*moduleSize) = info[i].imageLoadAddress.
            }

            // mach_msg_type_number_t size3 = PATH_MAX;

            // uint8_t* fpath_addr = readProcessMemory(pid,
            //         (mach_vm_address_t) info[i].imageFilePath, &size3);
            // if (fpath_addr)
            //     printf("path: %s %d %#010x\n",fpath_addr , size3, info[i].imageLoadAddress);
        }
    }
    return 0;
}
