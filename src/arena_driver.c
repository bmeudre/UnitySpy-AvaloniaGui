/***************************************************************************//**
*  \file       arena_driver.c
*
*  \details    MTG Arena /proc/$pid/mem file proxy driver
*
*  \author     frcaton
*
*  \Tested with Ubuntu 21.04
*
*******************************************************************************/
#include <linux/kernel.h>
#include <linux/init.h>
#include <linux/module.h>
#include <linux/kdev_t.h>
#include <linux/fs.h>
#include <linux/cdev.h>
#include <linux/device.h>
#include <stdio.h>

dev_t dev = 0;
static struct class *dev_class;
static struct cdev arena_cdev;

/*
** Function Prototypes
*/

// static int      str_ends_with(const char *str, const char *suffix);
// static int      get_arena_pid();

static int      __init arena_driver_init(void);
static void     __exit arena_driver_exit(void);
static int      arena_open(struct inode *inode, struct file *file);
static int      arena_release(struct inode *inode, struct file *file);
static ssize_t  arena_read(struct file *filp, char __user *buf, size_t len,loff_t * off);

static struct file_operations fops =
{
    .owner      = THIS_MODULE,
    .read       = arena_read,
    .open       = arena_open,
    .release    = arena_release,
};

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
** This function will be called when we open the Device file
*/
static int arena_open(struct inode *inode, struct file *file)
{
    pr_info("Driver Open Function Called...!!!\n");
    return 0;
}

/*
** This function will be called when we close the Device file
*/
static int arena_release(struct inode *inode, struct file *file)
{
    pr_info("Driver Release Function Called...!!!\n");
    return 0;
}

/*
** This function will be called when we read the Device file
*/
static ssize_t arena_read(struct file *filp, char __user *buf, size_t len, loff_t *off)
{
    pr_info("Driver Read Function Called...!!!\n");
    return 0;
}


/*
** Module Init function
*/
static int __init arena_driver_init(void)
{
        /*Allocating Major number*/
        if((alloc_chrdev_region(&dev, 0, 1, "arena_Dev")) <0){
                pr_err("Cannot allocate major number\n");
                return -1;
        }
        pr_info("Major = %d Minor = %d \n",MAJOR(dev), MINOR(dev));

        /*Creating cdev structure*/
        cdev_init(&arena_cdev,&fops);

        /*Adding character device to the system*/
        if((cdev_add(&arena_cdev,dev,1)) < 0){
            pr_err("Cannot add the device to the system\n");
            goto r_class;
        }

        /*Creating struct class*/
        if((dev_class = class_create(THIS_MODULE,"arena_class")) == NULL){
            pr_err("Cannot create the struct class\n");
            goto r_class;
        }

        /*Creating device*/
        if((device_create(dev_class,NULL,dev,NULL,"arena_device")) == NULL){
            pr_err("Cannot create the Device 1\n");
            goto r_device;
        }
        pr_info("MTG Arena Device Driver Insert...Done!!!\n");
      return 0;

r_device:
        class_destroy(dev_class);
r_class:
        unregister_chrdev_region(dev,1);
        return -1;
}

/*
** Module exit function
*/
static void __exit arena_driver_exit(void)
{
        device_destroy(dev_class,dev);
        class_destroy(dev_class);
        cdev_del(&arena_cdev);
        unregister_chrdev_region(dev, 1);
        pr_info("MTG Arena Device Driver Remove...Done!!!\n");
}

module_init(arena_driver_init);
module_exit(arena_driver_exit);

MODULE_LICENSE("GPL");
MODULE_AUTHOR("frcaton <weemanar@gmail.com>");
MODULE_DESCRIPTION("MTG Arena /proc/$pid/mem file proxy driver");
MODULE_VERSION("1.0");
