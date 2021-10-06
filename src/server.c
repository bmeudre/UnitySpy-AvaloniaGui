#include <stdio.h>
#include <dirent.h>
#include <sys/types.h>
#include <string.h>	//strlen
#include <sys/socket.h>
#include <arpa/inet.h>	//inet_addr
#include <unistd.h>	//write
#include <pthread.h> //for threading, link with lpthread

#define PORT      39185

#define REQUEST_SIZE 12 // byte 0 = request type, byte 1-8 address, byte 9-12 size
#define BUFFER_SIZE 4096 // byte 0 = request type, byte 1-8 address, byte 9-12 size

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

int get_arena_pid()
{
    int pid = 0;
    DIR *dir;
    struct dirent *ptr;
    FILE *fp;
    char filepath[50];//The size is arbitrary, can hold the path of cmdline file
    char cur_task_name[BUFFER_SIZE];//The size is arbitrary, can hold to recognize the command line text
    char buf[BUFFER_SIZE];
    dir = opendir("/proc"); //Open the path to the
    if (NULL != dir)
    {
        while ((ptr = readdir(dir)) != NULL) //Loop reads each file/folder in the path
        {
            //If it reads "." or ".." Skip, and skip the folder name if it is not read
            if ((strcmp(ptr->d_name, ".") == 0) || (strcmp(ptr->d_name, "..") == 0))  
            {
                continue;
            }
            if (DT_DIR != ptr->d_type) 
            {
                continue;
            }

            sprintf(filepath, "/proc/%s/status", ptr->d_name);//Generates the path to the file to be read            
            
            fp = fopen(filepath, "r");//Open the file
            if (NULL != fp)
            {
                if (fgets(buf, BUFFER_SIZE-1, fp)== NULL) 
                {
                    fclose(fp);
                    continue;
                }
                sscanf(buf, "%*s %s", cur_task_name);
            
                //Print the name of the path (that is, the PID of the process) if the file content meets the requirement
                if (str_ends_with(cur_task_name, "MTGA.exe"))
                {
                    pid = atoi(ptr->d_name);
                }
                fclose(fp);
            }

        }
        closedir(dir);//Shut down the path
    }
    else
    {
        perror("Could not open /proc");
    }
    return pid;
}

/*
 * This will handle connection for each client
 * */
void *connection_handler(void* socket_ptr)
{    
    int socket = *(int*)socket_ptr;
    int arena_pid;
    char buff[BUFFER_SIZE]; 
    char filepath[50];
    char request[REQUEST_SIZE]; 
    u_int64_t* address = request;
    int32_t* mem_size = request + 8;

    //Receive a request from the client
    if(recv(socket, request, REQUEST_SIZE, 0) < 0)
    {
        puts("recv failed");
    }
    else
    {
        arena_pid = get_arena_pid();

        //Generates the path to the file to be read
        sprintf(filepath, "/proc/%i/mem", arena_pid);

        FILE *fp = fopen(filepath, "r");
        if(fp == NULL)
        {
            perror("Open file error");
            printf("File: %s, arena_pid as i: %i, arena_pid as u: %u\n", filepath, arena_pid, arena_pid);  
        }
        else
        {                                
            size_t bytes_to_read;
            size_t remaining_bytes;
            do
            {      
                printf("Memory chunk requested: Address = %08X, size = %i\n", *address, *mem_size);  

                fseek(fp, *address, SEEK_SET);
                remaining_bytes = *mem_size;
                while(remaining_bytes > 0)
                {                 
                    if(remaining_bytes < BUFFER_SIZE)
                    {
                        bytes_to_read = remaining_bytes;
                    } 
                    else
                    {
                        bytes_to_read = BUFFER_SIZE;
                    }
                    bytes_to_read = fread(buff, sizeof(char), bytes_to_read, fp);
                    remaining_bytes -= bytes_to_read;
                    if(send(socket, buff, bytes_to_read, 0) < 0)
                    {
                        perror("Fail to send file");
                        break;
                    }
                }
                printf("memory chunk sent to the client!\n");
            } while(recv(socket, request, REQUEST_SIZE, 0) >= 0);

            puts("recv failed");
            fclose(fp);
        }            
    }
    
    puts("Closing connection");
    close(socket);
    fflush(stdout);
} 

int main(int argc, char *argv[])
{
	int socket_desc, new_socket, c;
	struct sockaddr_in server, client;
	pthread_t thread_id;
	
	//Create socket
	socket_desc = socket(AF_INET, SOCK_STREAM, 0);
	if (socket_desc == -1)
	{
		printf("Could not create socket");
	}
	
	//Prepare the sockaddr_in structure
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = INADDR_ANY;
	server.sin_port = htons( PORT );
	
	//Bind
	if( bind(socket_desc,(struct sockaddr *)&server , sizeof(server)) < 0)
	{
		puts("bind failed");
		return 1;
	}
	puts("bind done");
	
	//Listen
	listen(socket_desc , 3);
	
	//Accept and incoming connection
	puts("Waiting for incoming connections...");
	c = sizeof(struct sockaddr_in);
	while( (new_socket = accept(socket_desc, (struct sockaddr *)&client, (socklen_t*)&c)) )
	{		
        if( pthread_create( &thread_id , NULL ,  connection_handler , (void*) &new_socket) < 0)
        {
            perror("could not create thread");
            return 1;
        }
	}
	
    puts("Exiting...");
    fflush(stdout);
    
    if (new_socket < 0)
	{
		perror("accept failed");
		return 1;
	}

	
	return 0;
}