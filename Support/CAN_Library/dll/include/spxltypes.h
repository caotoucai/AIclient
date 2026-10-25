#ifndef SPXLTYPES_H
#define SPXLTYPES_H

#include <stdint.h>
#include <stddef.h>

#define SPXL_CAN_MAX_SIZE           64
#define SPXL_MSG_MAX_SIZE           SPXL_CAN_MAX_SIZE
#define SPXL_ERRBUF_SIZE            256
#define SPXL_STR_SIZE               128
#define SET_BIT(data, bit) 			(data) = (data)|(1<<(bit))

#define PROTOCOL_ID_FLEXRAY         0xE0
#define COMMAND_MASK		 		0x04
#define TXRX_MASK                   0x02
#define TIMESTAMP_MASK				0x01


enum {
    SPXL_MSGFLAG_EXTRADATA,
    SPXL_MSGFLAG_DEVREPORT              // message returned from the device
};

enum {
    SPXL_RET_ERR_UNKNOWN            = -1,
    SPXL_RET_SUCCESS                = 0,
    SPXL_RET_ERR_NOMSG,
    SPXL_RET_ERR_MSG_BUFOVERFLOW
};

enum {
    SPXL_INPUT_MODE_FRMSTREAM       = 0,
    SPXL_INPUT_MODE_FRMSIGNLEPOINT,
    SPXL_INPUT_MODE_SIGSIGNLEPOINT,
};

typedef char xl_int8_t;
typedef unsigned char xl_uint8_t;
typedef short xl_int16_t;
typedef unsigned short xl_uint16_t;
typedef int xl_int32_t;                 ///< 32-bit integer
typedef unsigned int xl_uint32_t;       ///< 32-bit unsigned integer
typedef long long xl_int64_t;
typedef unsigned long long xl_uint64_t;

typedef short xl_status_t;
typedef short xl_handle_t, *xl_handle_ptr;

enum class FRTxMode : xl_uint8_t {
    SPXL_eEvent,
    SPXL_eCyclic
};

enum FRConstants {
    SPXL_FR_BIT_TXMODE       = 2,
};

#ifndef _WIN64
#pragma pack(push, 1)
#endif

#define SPXL_PMSG_MAX_COUNT         16
typedef struct xl_pmsg
{
    int index;
    xl_uint8_t 	enable;
    xl_uint16_t period;
    xl_uint16_t delay;
    xl_uint8_t netProtocol;
    xl_uint8_t netInstance;
    xl_uint8_t isFD;    // 1 enable, 0 disable
    xl_uint32_t id;
    xl_uint8_t length;  // data length
    char data[SPXL_CAN_MAX_SIZE];
} xl_pmsg_t;

#ifndef DISABLE_IO_CONVENTER
typedef struct xl_mpmsg_iocfg
{
    xl_uint8_t enable;
    xl_uint8_t portIndex;
    xl_uint8_t function;
    xl_uint16_t pulse;
    xl_uint16_t period;
    xl_uint8_t factor[8];
    xl_uint8_t offset[8];
} xl_mpmsg_iocfg_t;

typedef struct xl_mpmsg_sigcfg
{
    xl_uint8_t startBit;
    xl_uint8_t len;
    xl_uint8_t byteorder;
    xl_uint8_t valueType;
    xl_uint8_t factor[8];
    xl_uint8_t offset[8];
    xl_uint32_t minRawValueH;
    xl_uint32_t minRawValueL;
    xl_uint32_t maxRawValueH;
    xl_uint32_t maxRawValueL;
} xl_mpmsg_sigcfg_t;

typedef struct xl_mpmsg_msgcfg
{
    xl_uint8_t enable;
    xl_uint8_t netProtocol;
    xl_uint8_t netInstance;
    xl_uint16_t period;
    xl_uint16_t delay;
    xl_uint16_t len;    //ID和数据总长度
    xl_int8_t data[64]; //ID + 数据
} xl_mpmsg_msgcfg_t;

#endif

#ifndef _WIN64
#pragma pack(pop)
#endif

#ifdef _MSC_VER
#define STRNCPY strncpy_s
#else
#define STRNCPY strncpy
#endif

#endif // SPXLTYPES_H
