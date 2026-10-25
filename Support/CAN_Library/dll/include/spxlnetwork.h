#ifndef SPXLNETWORK_H
#define SPXLNETWORK_H

#include <stdint.h>
#include "icommdefine.h"

enum {
    SPXL_PROTOCOL_INVALID       = 0x00,
    SPXL_PROTOCOL_CAN,
    SPXL_PROTOCOL_CANFD,
    SPXL_PROTOCOL_ISO15765_2,
    SPXL_PROTOCOL_LIN,
    SPXL_PROTOCOL_ETH,
    SPXL_PROTOCOL_FR,
    SPXL_PROTOCOL_GBLSETTINGS   = 0x1F
};

enum {
    SPXL_NETTYPE_UNKNOWN        = 0,
    SPXL_NETTYPE_DEVICECMD      = (1U << 0),
    SPXL_NETTYPE_DATA           = (1U << 1)
};

#endif // SPXLNETWORK_H
