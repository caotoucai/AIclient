#ifndef DEVICE_H
#define DEVICE_H

#include "spxlib_global.h"
#include "spxldevice.h"
#include "encoder.h"
#include "decoder.h"
#include "packetizer.h"
#include "serializer.h"
#include "icommunication.h"
#include "iconnection.h"
#include "message.h"
#include "idevicesettings.h"

namespace sapa {

class Device
{
public:
    virtual ~Device();

    virtual bool open();
    virtual bool close();

    const xl_device_t &getXlDevice() const {
        return m_data;
    }

    bool isConnected();
    bool transmit(std::shared_ptr<Message> message);
    bool transmit_rx(std::shared_ptr<Message> message, uint32_t rxMsgId, xl_msg_t *rxMsg, int timeout);
    bool transmit_rx_timeout(std::shared_ptr<Message> message, uint32_t rxMsgId, xl_msg_t *rxMsg, int timeout);
    void enableMessagePolling();
    bool disableMessagePolling();
    bool flushMessagePolling();
    std::vector<std::shared_ptr<Message>> getMessages();
    bool getMessages(std::vector<std::shared_ptr<Message>>& container, size_t limit = 0, std::chrono::milliseconds timeout = std::chrono::milliseconds(0));
    size_t getCurrentMessageCount() { return m_pollingContainer.size_approx(); }
    size_t getPollingMessageLimit() { return m_pollingMessageLimit; }
    void setPollingMessageLimit(size_t newSize) {
        m_pollingMessageLimit = newSize;
        enforcePollingMessageLimit();
    }
    bool disableMessagePollingWithCanMsgGet(uint32_t id, xl_msg_t *rxMsg);

    std::shared_ptr<ICommunication> m_com;
    std::unique_ptr<IDeviceSettings> m_settings;

protected:
    Device(xl_device_t device = { nullptr, {0} });

    template<typename Transport, typename Settings = NullSettings>
    void initialize() {
        auto transport = makeTransport<Transport>();
        setupTransport(*transport);
        auto serializer = makeSerializer();
        setupSerializer(*serializer);
        auto packetizer = makePacketizer(serializer);
        setupPacketizer(*packetizer);
        auto encoder = makeEncoder(packetizer);
        setupEncoder(*encoder);
        auto decoder = makeDecoder(serializer);
        setupDecoder(*decoder);
        m_com = makeCommunication(m_data.serial,
                                  std::move(transport),
                                  packetizer,
                                  serializer,
                                  std::move(encoder),
                                  std::move(decoder));
        setupCommunication(*m_com);
        m_settings = makeSettings<Settings>(m_com);
    }

    template<typename Transport>
    std::unique_ptr<IConnection> makeTransport() {
        return std::unique_ptr<IConnection>(new Transport(getWritableXlDevice()));
    }
    virtual void setupTransport(IConnection&) {}

    virtual std::shared_ptr<Packetizer> makePacketizer(std::shared_ptr<Serializer> s) {
        return std::make_shared<Packetizer>(s);
    }
    virtual void setupPacketizer(Packetizer&) {}

    virtual std::shared_ptr<Serializer> makeSerializer() {
        return std::make_shared<Serializer>();
    }
    virtual void setupSerializer(Serializer&) {}

    virtual std::unique_ptr<Encoder> makeEncoder(std::shared_ptr<Packetizer> p) {
        return std::unique_ptr<Encoder>(new Encoder(p));
    }
    virtual void setupEncoder(Encoder&) {}

    virtual std::unique_ptr<Decoder> makeDecoder(std::shared_ptr<Serializer> s) {
        return std::unique_ptr<Decoder>(new Decoder(s));
    }
    virtual void setupDecoder(Decoder&) {}

    virtual std::shared_ptr<ICommunication> makeCommunication(
        std::string serial,
        std::unique_ptr<IConnection> t,
        std::shared_ptr<Packetizer> p,
        std::shared_ptr<Serializer> s,
        std::unique_ptr<Encoder> e,
        std::unique_ptr<Decoder> d) {
            return std::make_shared<ICommunication>(serial, std::move(t), p, s, std::move(e), std::move(d));
    }
    virtual void setupCommunication(ICommunication&) {}

    template<typename Settings>
    std::unique_ptr<IDeviceSettings> makeSettings(std::shared_ptr<ICommunication> com) {
        return std::unique_ptr<IDeviceSettings>(new Settings(com));
    }
    virtual void setupSettings(IDeviceSettings&) {}

    xl_device_t& getWritableXlDevice() { return m_data; }

    int m_messagePollingCallbackID = 0;
    int m_internalHandlerCallbackID = 0;

private:
    void enforcePollingMessageLimit();

    size_t m_pollingMessageLimit = 20000;
    moodycamel::BlockingConcurrentQueue<std::shared_ptr<Message>> m_pollingContainer;
    xl_device_t m_data;
};



} // namespace sapa

#endif // DEVICE_H
