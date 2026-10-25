#ifndef MESSAGE_H
#define MESSAGE_H

#include "network.h"

#include <stdint.h>
#include <vector>

namespace sapa {

class Message {
public:
    enum CompleteCodeId {
        eCompleteCode_TXOK      = 0x08,
        eCompleteCode_RXOK      = 0x10
    };
    virtual ~Message() = default;
    bool isTxFrame() const {return m_network.isTx();}
    bool isCommand() const {return m_network.isCommand();}
    bool isCompleteCodeAppended() const {return m_isCompleteCodeAppended;}
    bool isTimeStampAppended() const {return m_network.isTimeStampAppended();}

    Network m_network;  // has extracted the raw header
    std::vector<uint8_t> m_data; // only start from the message data
    uint64_t m_timestamp{0}; // micro seconds

    // device commands has no complete code, only data streams have
    bool m_isCompleteCodeAppended{true};
    uint8_t m_completeCode;
#ifdef J2534_SUPPORT
    std::shared_ptr<void> m_optObjectPtr; // optional object Ptr
#endif	
};

}

#endif // MESSAGE_H
