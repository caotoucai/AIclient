#ifndef SPXLDEVICE_H
#define SPXLDEVICE_H

#include "spxltypes.h"

#define SPXL_DEVICE_SERIAL_LEN      16

#ifdef __cplusplus
// A forward declaration is needed as there is a circular dependency
namespace sapa {
class Device;
}
typedef sapa::Device* xl_device_handle_t;
#else
typedef void* xl_device_handle_t;
#endif

#pragma pack(push, 1)

typedef struct xl_device {
    xl_device_handle_t device;
    char serial[SPXL_DEVICE_SERIAL_LEN];
} xl_device_t;

#pragma pack(pop)

#endif // SPXLDEVICE_H
