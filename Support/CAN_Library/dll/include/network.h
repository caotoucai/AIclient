#ifndef NETWORK_H
#define NETWORK_H

#include "icommdefine.h"

#include <cstdint>
#include <vector>
#include <memory>

namespace sapa {

class NetId {
public:
    NetId() {}

    NetId(uint8_t protocol, uint8_t inst = 0)
        : m_protocol(protocol)
        , m_instance(inst)
    {}

    bool operator ==(const NetId& other) const
    {
        return ((m_protocol == other.m_protocol)
                && (m_instance == other.m_instance));
    }

    bool operator !=(const NetId& other) const
    {
        return !(*this == other);
    }

    bool operator <(const NetId& other) const
    {
        bool result = false;
        if (((m_protocol<<8) | m_instance) < ((other.m_protocol<<8) | other.m_instance)) {
            result = true;
        }
        return result;
    }

    uint8_t m_protocol{0};
    uint8_t m_instance{0};
};

class Network {
public:

    enum {
        BUS_PROTOCOL_INVALID        = 0x00,
        BUS_PROTOCOL_CAN,
        BUS_PROTOCOL_CANFD,
        BUS_PROTOCOL_ISO15765_2,
        BUS_PROTOCOL_LIN,
        BUS_PROTOCOL_ETH,
        BUS_PROTOCOL_FR,
        BUS_PROTOCOL_IO_CONVENTER,
        BUS_PROTOCOL_GBLSETTINGS    = 0x0F,
    };

    /*!
     * \brief The Type enum
     *
     * We use each bit to indicate a network type, so that we can merge them
     * when filtering
     *
     */
    enum class Type : uint32_t {
        eInvalid            = 0,
        eDeviceCmd,                 // include global settings and protocol commands
        eData,
        eAny                = 0xFFFFFFFE,
        eOther              = 0xFFFFFFFF
    };

    static const NetId AnyNetId;

    Network() {}

    std::vector<uint8_t> getNetHeader() const { return m_value; }
    NetId getNetId() const {return m_netid;}
    Type getType() const { return m_type; }
    void setType(Type type) { m_type = type; }
    uint8_t getProtocol() const { return m_netid.m_protocol; }
    uint8_t getInstance() const { return m_netid.m_instance; }
    bool isCommand() const {return m_isCommand;}
    bool isTx() const {return m_isTx;}
    bool isTimeStampAppended() const {return m_timestampSize != 0;}
    void setValue(const std::vector<uint8_t> &vec);
    int headerSize() const {return static_cast<int>(m_value.size());}
    int timestampSize() const {return m_timestampSize;}

    static bool isBusCAN(uint8_t protocol) {return protocol == BUS_PROTOCOL_CAN;}
    static bool isBusCANFD(uint8_t protocol) {return protocol == BUS_PROTOCOL_CANFD;}
    static bool isBusLIN(uint8_t protocol) {return protocol == BUS_PROTOCOL_LIN;}
    static bool isBusFR(uint8_t protocol) {return protocol == BUS_PROTOCOL_FR;}
    static bool isBusETH(uint8_t protocol) {return protocol == BUS_PROTOCOL_ETH;}

    static Type NetID2Type(uint8_t id);

public:
    std::vector<uint8_t> m_value; // Always use setValue so that m_value and m_type stay in sync
    Type m_type{Type::eInvalid};
    NetId m_netid;
    bool m_isCommand{false};
    bool m_isTx{true};
    bool m_isExtHdr{false};
    int m_timestampSize{0};
};

} // namespace sapa

#endif // NETWORK_H
