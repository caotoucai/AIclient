#ifndef CANPACKET_H
#define CANPACKET_H

#include "icommdefine.h"
#include "spxlmessage.h"
#include "canmessage.h"
#include "linmessage.h"

namespace sapa {

class Serializer;

class CANPacket
{
public:
//    enum {
//        eCanCfgHdr                          = PROTOCOL_ID_CONFIGURATION,
//        eCanCmdHdrStdCAN1                   = (PROTOCOL_ID_CAN1 | COMMAND_MASK),
//        eCanCmdHdrStdCAN2                   = (PROTOCOL_ID_CAN2 | COMMAND_MASK),
//        eFrCmdHdr                           = (PROTOCOL_ID_FLEXRAY | COMMAND_MASK)
//    };

    CANPacket() {}

    //static bool decodeToMessage(std::shared_ptr<CANMessage> &msg, const std::vector<uint8_t> &byteStream, const std::shared_ptr<Serializer> &serializer);
    //static std::shared_ptr<CANMessage> decodeToMessage(const std::vector<uint8_t>& byteStream);
    static bool encodeFromMessage(const CANMessage& message, std::vector<uint8_t>& byteStream);

    static void buildHeader(std::vector<uint8_t> &byteStream, uint32_t id, Network net, bool isRemote);
    static uint8_t dataSize2Dlc(int32_t dataSize);
    static int32_t dlc2DataSize(uint8_t dlc);

    static std::vector<uint8_t> getBaudRateDataByValue(int64_t baudrate);
    static int64_t getBaudRateValueByData(const std::vector<uint8_t> &data);
    static int64_t getfdArbitrationRateValueByIndex(uint8_t index);
    static int getfdArbitrationRateIndexByValue(int64_t value);
    static int64_t getfdDataRateValueByIndex(uint8_t index);
    static int getfdDataRateIndexByValue(int64_t value);

};

}

#endif // CANPACKET_H
