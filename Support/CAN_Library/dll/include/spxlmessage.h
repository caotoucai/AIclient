#ifndef SPXLMESSAGE_H
#define SPXLMESSAGE_H

#include "spxltypes.h"

//#include "FBFrame.h"
#ifndef _WIN64
#pragma pack(push)
#pragma pack(1)
#endif
#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable : 4201) // nameless struct/union
#endif
typedef union {
    struct {
        // CAN flags
        uint32_t complete_code : 8;     /**< The status code reported by device */
        uint32_t is_can_fd : 1;         /**< Indicate it's a CAN FD or Classic frame */
        uint32_t is_can_extended : 1;   /**< Indicate CAN ID is 29bit or 11bit */
        uint32_t is_frflag_startup : 1; /**< Indicate Flexray flag Startup */
        uint32_t is_frflag_sync : 1;    /**< Indicate Flexray flag Sync */
        uint32_t is_frflag_nf : 1;      /**< Indicate Flexray flag NullFrame */
        uint32_t is_frflag_pp : 1;      /**< Indicate Flexray flag PayloadPreamble */
        uint32_t : 18;
        uint32_t : 32;
        uint32_t : 32;
        uint32_t : 32;
    } B;
    uint32_t ST[4];
} xl_msg_statusbits_field_t;
#ifdef _MSC_VER
#pragma warning(pop)
#endif

typedef struct xl_msg {
    xl_msg_statusbits_field_t flags;    /**< Message attribute flags */
    xl_uint64_t timestamp;              /**< Timestamp of the received frame */
    xl_uint64_t timestampReserved;
    xl_uint8_t netProtocol;             /**< Network protocol of the frame, which defined in file spxlnetwork.h */
    xl_uint8_t netInstance;
    xl_uint32_t type;                   /**< Network type of the frame, which defined in file spxlnetwork.h */
    xl_uint8_t header[4];               /**< Protocol header */
    xl_uint8_t *data;                   /**< Message data pointer */
    size_t length;                      /**< Message data length */
    xl_uint8_t reserved[32];
} xl_msg_t;

typedef struct xl_msg_can {
    xl_msg_statusbits_field_t flags;    /**< Message attribute flags */
    xl_uint64_t timestamp;              /**< Timestamp of the received frame */
    xl_uint64_t timestampReserved;
    xl_uint8_t netProtocol;             /**< Network protocol of the frame, which defined in file spxlnetwork.h */
    xl_uint8_t netInstance;
    xl_uint32_t type;                   /**< Network type of the frame, which defined in file spxlnetwork.h */
    xl_uint32_t arbid;                  /**< CAN ID */
    xl_uint8_t *data;                   /**< Message data pointer */
    size_t length;                      /**< Message data length */
    xl_uint8_t bus;                     /**< CAN bus index, 0 is CAN1 and 1 is CAN2; The value should be auto set by netid */
    xl_uint8_t tx;                      /**< Is message to be transmitted or received */
    xl_uint8_t dlc;                     /**< CAN message DLC value */
    xl_uint8_t reserved[29];
    //xl_uint8_t *bp;   // big data pointer, when data size bigger than SPXL_CAN_MAX_SIZE, it stored here
} xl_msg_can_t;

/**
 * @brief reference to xl_msg_can structure
 *
 */
typedef struct xl_msg_lin {
    xl_msg_statusbits_field_t flags;
    xl_uint64_t timestamp;
    xl_uint64_t timestampReserved;
    xl_uint8_t netProtocol;
    xl_uint8_t netInstance;
    xl_uint32_t type;
    xl_uint32_t id;
    xl_uint8_t *data;
    size_t length;
    xl_uint8_t bus;
    xl_uint8_t tx;
    xl_uint8_t reserved[30];
    //xl_uint8_t *bp;   // big data pointer, when data size bigger than SPXL_CAN_MAX_SIZE, it stored here
} xl_msg_lin_t;

const xl_uint16_t FRMSG_INVALID_INDEX = 0xFFFF;
typedef struct xl_msg_fr {
    xl_msg_statusbits_field_t flags;
    xl_uint64_t timestamp;
    xl_uint64_t timestampReserved;
    xl_uint8_t netProtocol;
    xl_uint8_t netInstance;
    xl_uint32_t type;
    xl_uint32_t slotId;
    xl_uint8_t *data;
    size_t length;
    xl_uint8_t bus;
    xl_uint8_t tx;
    xl_uint8_t offset; //baseCycle
    xl_uint8_t repetition;
    xl_uint8_t cycleCount;
    xl_uint8_t channel;
    xl_uint8_t txMode;    //0:event  1:time trigger
    xl_uint16_t index;
    xl_int8_t *name;
    xl_uint8_t nameLength;
//    xl_uint8_t isTimeTrigger;  // 1:time trigger   0:event
#ifndef _WIN64
    xl_uint8_t reserved[18];  //64bit:7  32bit:18
#else
    xl_uint8_t reserved[7];  //64bit:7  32bit:18
#endif
    //xl_uint8_t *bp;   // big data pointer, when data size bigger than SPXL_CAN_MAX_SIZE, it stored here
} xl_msg_fr_t;

#ifndef _WIN64
#pragma pack(pop)
#endif

#ifdef __cplusplus
#include "message.h"
#include <memory>

//#define SPXL_MSG_STRUCTURE_SIZE (16+8+8+SPXL_MSG_MAX_SIZE+4+2+1+17)
#ifndef _WIN64
#define SPXL_MSG_STRUCTURE_SIZE (16+8+8+1+1+4+4+32)
#else
#define SPXL_MSG_STRUCTURE_SIZE (16+8+8+1+1+2+4+4+4+4*8)
#endif
static_assert(sizeof(xl_msg_t) == (SPXL_MSG_STRUCTURE_SIZE + sizeof(xl_uint8_t*) + sizeof(size_t)), "xl_msg_t size is incorrect! Changing size will break compatibility with existing C API programs.");
static_assert(sizeof(xl_msg_can_t) == sizeof(xl_msg_t), "All types of xl_msg_t must be the same size! (CAN is not)");
static_assert(sizeof(xl_msg_fr_t) == sizeof(xl_msg_t), "All types of xl_msg_t must be the same size! (FlexRay is not)");
static_assert(sizeof(xl_msg_lin_t) == sizeof(xl_msg_t), "All types of xl_msg_t must be the same size! (LIN is not)");

namespace sapa {

xl_msg_t createXlMessage(const std::shared_ptr<Message> message);
xl_msg_t createXlMessageWithDataObject(const std::shared_ptr<Message> message);
std::shared_ptr<Message> createMessageFromXlMessage(const xl_msg_t* xlMessage, const void *sptr);
bool getLastCanMessage(const std::vector<std::shared_ptr<Message>> messages, uint32_t id, xl_msg_t *rxMsg);
}
#endif

#endif // SPXLMESSAGE_H
