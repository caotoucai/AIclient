#ifndef SPXLIBC_H
#define SPXLIBC_H

#include "spxlib_global.h"
#include "spxltypes.h"
#include "spxldevice.h"
#include "spxlmessage.h"
#include "spxlnetwork.h"

#ifdef _MSC_VER
#include "Windows.h"
#else
#include "windows.h"
#endif

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Not used
 */
typedef struct xl_devinfo {
    struct xl_devinfo *next;
    char serial[SPXL_DEVICE_SERIAL_LEN];
} xl_devinfo_t;

/*
 * Not used
 */
typedef struct xl_signal {
    char name[SPXL_STR_SIZE];
} xl_signal_t;


struct FibexFrameInfo;
struct FibexSignalInfo;
/*!
 * spxlibc.h
 * <pre>
 * void spxl_get_libversion(char *version, size_t length);
 * </pre>
 *
 * Get spxlib version.
 *
 * \param version The version string of this library
 * \param length Length of the version string
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    char str[256];
    int ret;

    ret = spxl_get_libversion(str, sizeof(str));
    if (ret == 0) {
        // print lib version from str
    }
}
</pre>
 * \defgroup spxl_get_libversion spxl_get_libversion
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_get_libversion(char *version, size_t length);

/*!
 * spxlibc.h
 * <pre>
 * void spxl_init(void);
 * </pre>
 *
 * Init the spxl library and configure log module to none output. This function
 * should be called at first when you use the library.
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    spxl_init();
}
</pre>
 * \defgroup spxl_init spxl_init
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_init(void);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_init_debug(void);
 * </pre>
 *
 * Init the spxl library and configure log module to trace level output. Use
 * this function instead of spxl_init(), the library can record trace messages
 * in log file.
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    spxl_init_debug();
}
</pre>
 * \defgroup spxl_init_debug spxl_init_debug
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_init_debug(void);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_destroy(void);
 * </pre>
 *
 * Free spxl library resources. This function is not a MUST option.
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    spxl_init();
    // do some actions
    spxl_destroy();
}
</pre>
 * \defgroup spxl_destroy spxl_destroy
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_destroy(void);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_find_alldevs(xl_device_t *devices, size_t *count);
 * </pre>
 *
 * Find devices managered by iEngine with default address(127.0.0.1) and
 * port(8000).
 *
 * \param devices A pre-alloced xl_device_t array which will be filled by the function.
 * \param count INPUT xl_device_t array size, and OUTPUT actual device numbers.
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    size_t count = MAX_DEVICE_COUNT;

    xl_device_t *deviceList = reinterpret_cast<xl_device_t *>(calloc(MAX_DEVICE_COUNT, sizeof(xl_device_t)));
    int ret = spxl_find_alldevs(deviceList, &count);
    if (ret <= 0) {
        // No device found
        return;
    }
    // count devices found, do actions as you want
    // ...

    free(deviceList);
}
</pre>
 * \defgroup spxl_find_alldevs spxl_find_alldevs
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_find_alldevs(xl_device_t *devices, size_t *count);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_find_alldevs_ex(xl_device_t *devices,
 *                          size_t *count,
 *                          char *ip,
 *                          xl_uint16_t port);
 * </pre>
 *
 * Find devices managered by iEngine with spcific address and port.
 *
 * \param devices A pre-alloced xl_device_t array which will be filled by the function.
 * \param count The number of found devices
 * \param ip iEngine listening address
 * \param port iEngine listening port
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_find_alldevs_ex spxl_find_alldevs_ex
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_find_alldevs_ex(xl_device_t *devices,
                                             size_t *count,
                                             char *ip,
                                             xl_uint16_t port);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_flush_clients_by_device(const char *devname, char *ip, xl_uint16_t port);
 * </pre>
 *
 * Flush all client connections of specific device managered by iEngine with
 * specific address and port.
 *
 * \param devname Device name
 * \param ip iEngine listening address
 * \param port iEngine listening port
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_flush_clients_by_device spxl_flush_clients_by_device
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_flush_clients_by_device(const char *devname, char *ip, xl_uint16_t port);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_open(const xl_device_t *device);
 * </pre>
 *
 * Open a specific device
 *
 * \param device The xl_device_t pointer to a specific device
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    xl_device_t *device = deviceList + devIndex;
    int ret = spxl_open(device);
    if (ret != 0) {
        // device open failure
        return;
    }

    // ...
}
</pre>
 * \defgroup spxl_open spxl_open
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_open(const xl_device_t *device);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_close(const xl_device_t *device);
 * </pre>
 *
 * Close a specific device
 *
 * \param device The xl_device_t pointer to a specific device
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    int ret = spxl_close(device);
    if (ret != 0) {
        // device close failure
        return;
    }

    // ...
}
</pre>
 * \defgroup spxl_close spxl_close
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_close(const xl_device_t *device);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_transmit(const xl_device_t *device, const xl_msg_t *msg);
 * </pre>
 *
 * Transmit messages, which is not supported yet. The function works as spxl_transmit_one.
 *
 * \param device
 * \param msg
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    // transmit a LIN message
    xl_msg_lin_t msg;
    memset(&msg, 0, sizeof(msg));
    std::vector<uint8_t> msgData({0x01, 0x02});
    msg.netid = SPXL_NETID_LIN1;
    msg.id = 0x13;
    msg.data = msgData.data();
    msg.length = msgData.size();
    spxl_transmit(m_device, reinterpret_cast<xl_msg_t *>(&msg));
}
</pre>
 * \defgroup spxl_transmit spxl_transmit
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_transmit(const xl_device_t *device, const xl_msg_t *msg);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_transmit_one(const xl_device_t *device, xl_msg_t *msg, xl_uint8_t *data, size_t data_length);
 * </pre>
 *
 * Transmit a message to interface which defined in msg->netid
 *
 * \param device The xl_device_t pointer to a specific device
 * \param msg The message which is to be sent out
 * \param data the data pointer which will replace to msg->data
 * \param data_length the data length which will replace to msg->length
 *
 * message which should be send out, should be filled with following structures
 *
 * xl_msg_can_t - netid is SPXL_NETID_CAN1/2 or SPXL_NETID_CANFD1/2
 * xl_msg_lin_t - netid is SPXL_NETID_LIN1
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    xl_msg_can_t msg, rxmsg;
    memset(&msg, 0, sizeof(msg));
    std::vector<uint8_t> msgData({0x01, 0x02});
    msg.netid = SPXL_NETID_CANFD1;
    msg.arbid = 0x124;
    msg.length = msgData.size();
    ret = spxl_transmit_one(m_device, reinterpret_cast<xl_msg_t *>(&msg), msgData.data(), msgData.size());
}
</pre>
 * \defgroup spxl_transmit_one spxl_transmit_one
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_transmit_one(const xl_device_t *device, xl_msg_t *msg, xl_uint8_t *data, size_t data_length);

/*!
 * spxlibc.h
 * <pre>
 * spxl_transmit_rx(const xl_device_t *device,
 *                  xl_msg_t *txmsg,
 *                  xl_uint8_t *data,
 *                  xl_uint32_t rxmsg_id,
 *                  xl_msg_t *rxmsg,
 *                  xl_uint32_t timeout);
 * </pre>
 *
 * Transmit a message to interface which defined in msg->netid, then wait for
 * the first response message with specific id in timeout microseconds.
 * Becareful that you should free the recevied message data by manual.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param txmsg The message which is to be sent out
 * \param data The data pointer which will be set to txmsg.data
 * \param rxmsg_id The rx message id
 * \param rxmsg The message which is received
 * \param timeout ms value of waiting for the first rx message with rxmsg_id
 * \return
 *
 * Example usage:
<pre>
void foo( void )
{
    int ret;

    // Transmit a CAN message and get the first recevied one with ID 0x123 in
    // rxmsg.
    xl_msg_can_t msg, rxmsg;
    memset(&msg, 0, sizeof(msg));
    std::vector<uint8_t> msgData({0x01, 0x02});
    msg.netid = SPXL_NETID_CAN1;
    msg.arbid = 0x124;
    msg.data = msgData.data();
    msg.length = msgData.size();
    ret = spxl_transmit_rx(m_device, reinterpret_cast<xl_msg_t *>(&msg), msg.data, 0x123, reinterpret_cast<xl_msg_t *>(&rxmsg), 500);
    free(rxmsg.data);
}
</pre>
 * \defgroup spxl_transmit_rx spxl_transmit_rx
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_transmit_rx(const xl_device_t *device,
                                         xl_msg_t *txmsg,
                                         xl_uint8_t *data,
                                         xl_uint32_t rxmsg_id,
                                         xl_msg_t *rxmsg,
                                         xl_uint32_t timeout);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_receive(const xl_device_t *device,
 *                  xl_msg_t *msgs,
 *                  xl_uint32_t *count,
 *                  xl_uint32_t timeout);
 * </pre>
 *
 * Reads a message from the received buffer (NOTICE that only data messages
 * will be queued in the recevied buffer).
 *
 * When timeout is 0, if no message is available, the function returns
 * immediately with return code SPXL_RET_ERR_NOMSG.
 *
 * When timeout is not 0, then the function return when count messages received
 * or timeout.
 *
 * The driver object will hold 20,000 received messages before it will generate
 * an rx buffer overflow error
 *
 * It is the job of the application software to read receive buffer at regular
 * intervals. The rate that the application needs to read these messages is
 * dependant on the rate messages are received on the bus. For example, a high
 * bandwidth(500kbps) CAN bus can generate 5000 messages per second. In this
 * case you must read out the messages at least every four seconds or overflow
 * errors will result.
 *
 * An application should use 'polling' function to receive the messages.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param msg The received messages pointer
 * \param count The count of received messages
 * \param timeout timeout in ms which wait for count messages to be received.
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    spxl_enable_rx_polling(m_device);
    while (m_isRunning) {
        xl_msg_t rxMsg;
        uint32_t rxCount = 1;
        int ret;

        ret = spxl_receive(m_device, &rxMsg, &rxCount, 0);
        if (ret == 0) {
            switch (rxMsg.netProtocol) {
                case SPXL_PROTOCOL_CAN: {
                    xl_msg_can_t& can = *reinterpret_cast<xl_msg_can_t *>(&rxMsg);
                    // ...
                    break;
                }
                case SPXL_PROTOCOL_LIN: {
                    xl_msg_lin_t& lin = *reinterpret_cast<xl_msg_lin_t *>(&rxMsg);
                    // ...
                    break;
                }
                default:
                    break;
            }

        } else {
            // no frame received
        }
    }
}
</pre>
 * \defgroup spxl_receive spxl_receive
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_receive(const xl_device_t *device,
                                     xl_msg_t *msgs,
                                     xl_uint32_t *count,
                                     xl_uint32_t timeout);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_enable_rx_polling(const xl_device_t *device);
 * </pre>
 *
 * Enable message buffer for receiving messages. This function must be called
 * before spxl_receive
 *
 * \param device
 * \return
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_enable_rx_polling spxl_enable_rx_polling
 * \ingroup spxl_ctrl
 */
void SPXLIBSHARED_EXPORT spxl_enable_rx_polling(const xl_device_t *device);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_disable_rx_polling(const xl_device_t *device);
 * </pre>
 *
 * Disbale message buffer for receiving messages.
 *
 * \param device
 * \return
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_disable_rx_polling spxl_disable_rx_polling
 * \ingroup spxl_ctrl
 */
void SPXLIBSHARED_EXPORT spxl_disable_rx_polling(const xl_device_t *device);

int SPXLIBSHARED_EXPORT spxl_flush_rx_polling(const xl_device_t *device);

int SPXLIBSHARED_EXPORT spxl_set_fr_filter_to_rx_polling(const xl_device_t *device, const bool isBlackList, const char *frMsgNameh);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_disable_rx_polling_canmsg(const xl_device_t *device,
 *                                    xl_uint32_t rxmsg_id,
 *                                    xl_msg_t *rxmsg);
 * </pre>
 *
 * Disable the polloing function and search last indicate can message(by
 * rxmsg_id) in the current queue.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param rxmsg_id
 * \param rxmsg be careful that rxmsg->data must be allocated by user before pass in
 * \return
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_disable_rx_polling_canmsg spxl_disable_rx_polling_canmsg
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_disable_rx_polling_canmsg(const xl_device_t *device,
                                                       xl_uint32_t rxmsg_id,
                                                       xl_msg_t *rxmsg);

int SPXLIBSHARED_EXPORT spxl_set_input_mode(const xl_device_t *device, xl_uint8_t mode);
int SPXLIBSHARED_EXPORT spxl_set_recv_msgs(const xl_device_t *device, xl_msg_t *msgs);
int SPXLIBSHARED_EXPORT spxl_set_recv_signals(const xl_device_t *device, xl_signal_t *sigs);
int SPXLIBSHARED_EXPORT spxl_set_notification(const xl_device_t *device, HANDLE ev_handle);
int SPXLIBSHARED_EXPORT spxl_get_version(const xl_device_t *device, char *response, xl_uint32_t resplen);

/**
 * spxlibc.h
 * <pre>
 * int spxl_get_can_baudrate(const xl_device_t *device, int instance, xl_int64_t *baudrate);
 * </pre>
 *
 * get the specified CAN interface arbi baudrate of device
 *
 * @param device The xl_device_t pointer to a specific device
 * @param instance The instance number of the device (0-CAN1, 1-CAN2)
 * @param baudrate The baudrate of the bus
 * @return int 0 if successful, -1 otherwise.
 */
int SPXLIBSHARED_EXPORT spxl_get_can_baudrate(const xl_device_t *device, int instance, xl_int64_t *baudrate);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_set_baudrate(const xl_device_t *device, int bus, xl_int64_t baudrate);
 * </pre>
 *
 * set the specified CAN interface arbi baudrate of device
 *
 * \param device The xl_device_t pointer to a specific device
 * \param instance The instance number of the device (0-CAN1, 1-CAN2)
 *
 * \param baudrate support values are
 *
 * 33482, 83333, 95200, 100000, 125000, 200000, 250000, 500000, 1000000
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_set_can_baudrate spxl_set_can_baudrate
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_set_can_baudrate(const xl_device_t *device, int instance, xl_int64_t baudrate);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_get_periodic_messages(const xl_device_t *device,
 *                                xl_pmsg_t *pmsgs,
 *                                int *count,
 *                                xl_uint8_t protocol,
 *                                xl_uint8_t instance);
 * </pre>
 *
 * Get periodic messages which has been configured in the deivce memory.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param pmsgs A array of xl_pmsg_t structure
 * \param count The number of the returned xl_pmsg_t array
 * \param protocol The bus protocol, e.g. SPXL_PROTOCOL_CAN ..., refercence to spxlnetwork.h
 * \param instance The instance number of the specified bus protocol
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_get_periodic_messages spxl_get_periodic_messages
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_get_periodic_messages(const xl_device_t *device,
                                                   xl_pmsg_t *pmsgs,
                                                   int *count,
                                                   xl_uint8_t protocol,
                                                   xl_uint8_t instance);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_set_periodic_message(const xl_device_t *device, const xl_pmsg_t *pmsgs);
 * </pre>
 *
 * Set an specific periodic message in the deivce memory.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param pmsgs The xl_pmsg_t structure pointer to a specific message
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    xl_pmsg_t pmsg = {
        1, 0, 200, 1, SPXL_PROTOCOL_CAN, 0, 1, 0x456, 12,
        {0x00, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x21, 0x22}
    };
    spxl_set_periodic_message(m_device, &pmsg);
}
</pre>
 * \defgroup spxl_set_periodic_message spxl_set_periodic_message
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_set_periodic_message(const xl_device_t *device, const xl_pmsg_t *pmsgs);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_enable_periodic_message(const xl_device_t *device,
 *                                  int index,
 *                                  xl_uint8_t enable,
 *                                  xl_uint8_t protocol,
 *                                  xl_uint8_t instance);
 * </pre>
 *
 * Enable/Disable an specific periodic message in the deivce memory by index.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param index The index number of a specific message
 * \param enable 1 is enable, 0 is disable
 * \param protocol The bus protocol, e.g. SPXL_PROTOCOL_CAN ..., refercence to spxlnetwork.h
 * \param instance The instance number of the specified bus protocol
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_enable_periodic_message spxl_enable_periodic_message
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_enable_periodic_message(const xl_device_t *device,
                                                     int index,
                                                     xl_uint8_t enable,
                                                     xl_uint8_t protocol,
                                                     xl_uint8_t instance);

/*!
 * spxlibc.h
 * <pre>int spxl_delete_periodic_message(const xl_device_t *device,
 *                                       int index,
 *                                       xl_uint8_t protocol,
 *                                       xl_uint8_t instance);
 * </pre>
 *
 * Delete an specific periodic message in the deivce memory by index.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param index The index number of a specific message
 * \param protocol The bus protocol, e.g. SPXL_PROTOCOL_CAN ..., refercence to spxlnetwork.h
 * \param instance The instance number of the specified bus protocol
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_delete_periodic_message spxl_delete_periodic_message
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_delete_periodic_message(const xl_device_t *device,
                                                     int index,
                                                     xl_uint8_t protocol,
                                                     xl_uint8_t instance);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_delete_periodic_message_all(const xl_device_t *device,
 *                                      xl_uint8_t protocol,
 *                                      xl_uint8_t instance);
 * </pre>
 *
 * Delete all specific periodic messages in the deivce memory.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param protocol The bus protocol, e.g. SPXL_PROTOCOL_CAN ..., refercence to spxlnetwork.h
 * \param instance The instance number of the specified bus protocol
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_delete_periodic_message_all spxl_delete_periodic_message_all
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_delete_periodic_message_all(const xl_device_t *device,
                                                         xl_uint8_t protocol,
                                                         xl_uint8_t instance);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_get_canfd_options(const xl_device_t *device,
 *                            int instance,
 *                            xl_uint8_t *enable,
 *                            xl_uint8_t *brs,
 *                            xl_int64_t *arbi_rate,
 *                            xl_int64_t *data_rate);
 * </pre>
 *
 * Get the options of CAN FD settings.
 *
 * @param device The xl_device_t pointer to a specific device
 * @param instance The instance number of the specified bus protocol
 * @param enable CAN FD enable(1) or disable(0)
 * @param brs Baudrate switcher enable(1) or not(0)
 * @param arbi_rate
 * @param data_rate
 * @return int 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    uint8_t enable = 0, brs = 0;
    int64_t arbi_rate = 0, data_rate = 0;
    int ret = spxl_get_canfd_options(m_device, 0, &enable, &brs, &arbi_rate, &data_rate);
}
</pre>
 * \defgroup spxl_get_canfd_options spxl_get_canfd_options
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_get_canfd_options(const xl_device_t *device,
                                               int instance,
                                               xl_uint8_t *enable,
                                               xl_uint8_t *brs,
                                               xl_int64_t *arbi_rate,
                                               xl_int64_t *data_rate);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_set_canfd_options(const xl_device_t *device,
 *                            int instance,
 *                            xl_uint8_t enable,
 *                            xl_uint8_t brs,
 *                            xl_int64_t arbi_rate,
 *                            xl_int64_t data_rate);
 * </pre>
 *
 * Set canfd related options
 *
 * \param device The xl_device_t pointer to a specific device
 * \param instance The instance number of the specified bus protocol
 * \param enable
 * \param brs
 *
 * \param arbi_rate support values as following:
 *
 * 1000000, 800000, 500000, 400000, 250000, 200000, 160000, 125000, 100000,
 * 83000, 80000, 62500, 50000, 40000, 33330
 *
 * \param data_rate
 *
 * 5000000, 4000000, 3000000, 2500000, 2000000, 1600000, 1250000, 1000000,
 * 800000, 500000, 400000, 250000, 200000
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_set_canfd_options spxl_set_canfd_options
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_set_canfd_options(const xl_device_t *device,
                                               int instance,
                                               xl_uint8_t enable,
                                               xl_uint8_t brs,
                                               xl_int64_t arbi_rate,
                                               xl_int64_t data_rate);

int SPXLIBSHARED_EXPORT spxl_get_can_transceiver(const xl_device_t *device,
                                                 int bus,
                                                 xl_uint8_t *txvr,
                                                 xl_uint8_t *mode);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_set_can_transceiver(const xl_device_t *device,
 *                              int instance,
 *                              xl_uint8_t txvr,
 *                              xl_uint8_t mode);
 * </pre>
 *
 * \param device The xl_device_t pointer to a specific device
 * \param instance The instance number of CAN bus protocol
 * \param txvr The transceiver type to be set with, 0 - high speed, 1 - FT, 2 - single wired
 * \param mode
 *
 * 	txvr == 0; 0 - normal, 1 - standby
 *	txvr == 1; 1 - sleep, 3 - normal
 *  txvr == 2; 0 - sleep, 1 - hight speed, 2 - hight voltage, 3 - normal
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_set_can_transceiver spxl_set_can_transceiver
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_set_can_transceiver(const xl_device_t *device,
                                                 int instance,
                                                 xl_uint8_t txvr,
                                                 xl_uint8_t mode);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_command(const xl_device_t *device, const char *cmd, int cmdlen);
 * </pre>
 *
 * Send low level command to device in hex string format
 *
 * \param device The xl_device_t pointer to a specific device
 * \param cmd Hex string command, e.g. "0892" or "08 92"
 * \param cmdlen The length of the command string
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_send_command spxl_send_command
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_command(const xl_device_t *device, const char *cmd, int cmdlen);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_command_ex(const xl_device_t *device,
 *                          const char *cmd,
 *                          xl_uint32_t cmdlen,
 *                          xl_uint8_t *response,
 *                          xl_uint32_t *resplen);
 * </pre>
 *
 * Send low level command to device in hex string format
 *
 * \param device The xl_device_t pointer to a specific device
 * \param cmd Hex string command, e.g. "0892" or "08 92"
 * \param cmdlen The length of the command string
 * \param response The full content of response. If there are many response, we put it together.
 * \param resplen
 * \return
 *
 * Example usage:
<pre>
void foo( void )
{
    char cmd[] = "0892";
    uint8_t response[256];
    uint32_t responseLen = sizeof(response);
    int ret = spxl_send_command_ex(m_device, cmd, sizeof(cmd), response, &responseLen);
}
</pre>
 * \defgroup spxl_send_command_ex spxl_send_command_ex
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_command_ex(const xl_device_t *device,
                                             const char *cmd,
                                             xl_uint32_t cmdlen,
                                             xl_uint8_t *response,
                                             xl_uint32_t *resplen);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_isotp_enable(const xl_device_t *device);
 * </pre>
 *
 * Enable device isotp function. This function MUST to be called before using isotp function.
 *
 * \param device The xl_device_t pointer to a specific device
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_isotp_enable spxl_isotp_enable
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_isotp_enable(const xl_device_t *device, uint8_t instance);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_isotp_configure(const xl_device_t *device,
 *                          xl_uint8_t bus,
 *                          xl_uint32_t txid,
 *                          xl_uint32_t rxid,
 *                          xl_uint32_t txfcid,
 *                          xl_uint32_t rxfcid);
 * </pre>
 *
 * This function belongs to 15765-2 part. It sets the ids of 15765-2 protocol.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param txid The isotp tx id.
 * \param rxid The isotp rx id.
 * \param txfcid The tx flow control id.
 * \param rxfcid The rx flow control id.
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_isotp_configure spxl_isotp_configure
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_isotp_configure(const xl_device_t *device,
                                             xl_uint8_t bus,
                                             xl_uint32_t txid,
                                             xl_uint32_t rxid,
                                             xl_uint32_t txfcid,
                                             xl_uint32_t rxfcid);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_isotp_configure_ex(const xl_device_t *device,
 *                             xl_uint8_t instance,
 *                             xl_uint32_t txid, xl_uint8_t txxx, xl_uint8_t txyy,
 *                             xl_uint32_t rxid, xl_uint8_t rxxx,
 *                             xl_uint32_t txfcid, xl_uint8_t fcxx, xl_uint8_t fcyy, xl_uint8_t fczz,
 *                             xl_uint32_t rxfcid);
 * </pre>
 *
 * This function belongs to 15765-2 part. It sets full configurations of 15765-2 protocol.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param instance
 * \param txid The isotp tx id.
 * \param txxx
 * <pre>
 * bit 0 = 0: no frame padding
 * bit 0 = 1: frame padding with 0x00s
 * bit 1 = 0: use flow control STmin
 * bit 1 = 1: override flow control STmin with YY value
 * bit 2 = 0: tx msg contains no ISO15765-2 extended ID
 * bit 2 = 1: tx msg contains an ISO15765-2 extended ID
 * bit 3 = 0: display both individual CAN frames and the ISO TP message
 * bit 3 = 1: display only the ISO TP message
 * bit 4-7 = 0
 * </pre>
 * \param txyy The value used for STmin in ms if txxx, bit 1 = 1
 * \param rxid The isotp rx id.
 * \param rxxx
 * <pre>
 * 0x00 : individual CAN frames will be displayed as well as the ISO TP message
 * 0x01 : only the ISO TP message will be displayed
 * </pre>
 * \param txfcid The tx flow control id.
 * \param fcxx
 * <pre>
 * bit 0 = 0: no frame padding
 * bit 0 = 1: frame padding with 0x00s
 * bit 1 = 0:
 * bit 2 = 0:  FS = 0 clear to send consecutive frames
 * bit 2 = 1:  FS = 0 wait to send CFs
 * bit 3-7 = 0:
 * </pre>
 * \param fcyy The value used for BS (block size)
 * \param fczz The value used for STmin (separation time)
 * \param rxfcid The rx flow control id.
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_isotp_configure_ex spxl_isotp_configure_ex
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_isotp_configure_ex(const xl_device_t *device,
                                                xl_uint8_t instance,
                                                xl_uint32_t txid, xl_uint8_t txxx, xl_uint8_t txyy,
                                                xl_uint32_t rxid, xl_uint8_t rxxx, xl_uint32_t rxidmask, xl_uint8_t rxxd, xl_uint8_t rxxdmask,
                                                xl_uint32_t txfcid, xl_uint8_t fcxx, xl_uint8_t fcyy, xl_uint8_t fczz, xl_uint8_t fcxd,
                                                xl_uint32_t rxfcid);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_isotp_set_padding(const xl_device_t *device,
 *                            xl_uint8_t bus,
 *                            xl_uint8_t subcmd,
 *                            xl_uint8_t tx_frame_padding_value,
 *                            xl_uint8_t tx_fc_padding_value);
 * </pre>
 *
 * This function belongs to 15765-2 part. It sets padding values of 15765-2 protocol.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param bus
 * \param subcmd The command indicate which apdding value is to be set
 * <pre>
 * 0x01: set tx data frame padding value only(tx_fc_padding_value will be ignored)
 * 0x02: set tc flow control padding value only(tx_frame_padding_value will be ignored)
 * 0x03: set both tx frame padding value and tc flow control padding value
 * </pre>
 * \param tx_frame_padding_value The padding value of tx data frames
 * \param tx_fc_padding_value The padding value of tx flow control frames
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    // set both tx data frame padding and tx flow control frame padding to 0x55 on CAN1
    spxl_isotp_set_padding(device, 0, 0x03, 0x55, 0x55);
}
</pre>
 * \defgroup spxl_isotp_set_padding spxl_isotp_set_padding
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_isotp_set_padding(const xl_device_t *device,
                                               xl_uint8_t bus,
                                               xl_uint8_t subcmd,
                                               xl_uint8_t tx_frame_padding_value,
                                               xl_uint8_t tx_fc_padding_value);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_isotp_request(const xl_device_t *device,
 *                        xl_uint8_t instance, xl_uint8_t *request, xl_uint32_t reqlen,
 *                        xl_uint8_t *response, xl_uint32_t *resplen, xl_uint32_t timeout);
 * </pre>
 *
 * Send isotp(15765-2) frame to bus. The function will return both negative
 * and positive response.
 *
 * ex.
 * -> 10 02
 * <- 50 02 00 32 01 f4
 *
 * \param device The xl_device_t pointer to a specific device
 * \param instance
 * \param request The raw data which we will send out, it should include both the UDS service id and data.
 * \param reqlen The length of the request data.
 * \param response The raw data which we received.
 * \param resplen The length of the response data.
 * \param timeout The response wait timeout value.
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_isotp_request spxl_isotp_request
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_isotp_request(const xl_device_t *device,
                                           xl_uint8_t instance,
                                           xl_uint8_t *request,
                                           xl_uint32_t reqlen,
                                           xl_uint8_t *response,
                                           xl_uint32_t *resplen,
                                           xl_uint32_t timeout);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_enable_protocol(const xl_device_t *device, xl_uint8_t protocol);
 * </pre>
 *
 * Enable a specific protocol on the device.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param protocol The protocol value
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_enable_protocol spxl_enable_protocol
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_enable_protocol(const xl_device_t *device, xl_uint8_t protocol, xl_uint8_t instance);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_disable_protocol(const xl_device_t *device, xl_uint8_t protocol);
 * </pre>
 *
 * Disable a specific protocol on the device.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param protocol The protocol value
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_disable_protocol spxl_disable_protocol
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_disable_protocol(const xl_device_t *device, xl_uint8_t protocol, xl_uint8_t instance);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_enable_can(const xl_device_t *device, xl_uint8_t instance);
 * </pre>
 *
 * Enable a specific CAN interface on the device.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param instance 0 - CAN1, 1 - CAN2
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_enable_can spxl_enable_can
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_enable_can(const xl_device_t *device, xl_uint8_t instance);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_disable_can(const xl_device_t *device, xl_uint8_t instance);
 * </pre>
 *
 * Disable a specific CAN interface on the device.
 *
 * \param device The xl_device_t pointer to a specific device
 * \param instance 0 - CAN1, 1 - CAN2
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_disable_can spxl_disable_can
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_disable_can(const xl_device_t *device, xl_uint8_t instance);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_get_fr_controller_status(const xl_device_t *device,
 *                                   xl_uint8_t *code_start_noise,
 *                                   xl_uint8_t *chi_halt_request,
 *                                   xl_uint8_t *freeze,
 *                                   xl_uint8_t *slot_mode,
 *                                   xl_uint8_t *wakeup_status,
 *                                   xl_uint8_t *error_mode,
 *                                   xl_uint8_t *startup_status,
 *                                   xl_uint8_t *state);
 * </pre>
 *
 * Get the flexray controller status.
 *
 * @param device The xl_device_t pointer to a specific device
 * @param code_start_noise
 * <pre>
 * 1: means that noise detected on bus during startup
 * 0: otherwise
 * </pre>
 * @param chi_halt_request
 * <pre>
 * 1: means that there is pending halt request
 * 0: otherwise
 * </pre>
 * @param freeze
 * <pre>
 * 1: means that internal error causing transition to the POC:Halt state or FREEZE command occurred
 * 0: otherwise
 * </pre>
 * @param slot_mode Contains FlexRay controller slot mode
 * <pre>
 * 0: FR_SLOTMODE_KEYSLOT, FlexRay controller is transmitting only in single slot (key slot)
 * 1: FR_SLOTMODE_ALL_PENDING, After end of the cycle controller will change slot mode to FR_SLOTMODE_ALL
 * 2. FR_SLOTMODE_ALL, FlexRay controller transmits in all configured slots
 * </pre>
 * @param wakeup_status
 * <pre>
 * 0: FR_WAKEUP_UNDEFINED,          Unknown state - the controller has not been in POC:Wakeup state since it was in POC:Default Config
 * 1: FR_WAKEUP_RECEIVED_HEADER,    Frame header was received during initial listen phase
 * 2: FR_WAKEUP_RECEIVED_WUP,       Valid WUP was received during initial listen phase
 * 3: FR_WAKEUP_COLLISION_HEADER,   Collision with frame header during WUP transmission
 * 4: FR_WAKEUP_COLLISION_WUP,      Collision with another wakeup during WUP transmission
 * 5: FR_WAKEUP_COLLISION_UNKNOWN,  Unknown collision during WUP transmission
 * 6: FR_WAKEUP_TRANSMITTED         WUP has been successfully transmitted
 * </pre>
 * @param error_mode
 * <pre>
 * 0: FR_ERRORMODE_ACTIVE,  Controller is synchronized to FlexRay cluster. Synchronization has not been lost more than gMaxWithoutClockCorrectionPassive cycles.
 * 1: FR_ERRORMODE_PASSIVE, Controller has lost synchronization for less than gMaxWithoutClockCorrectionFatal cycles.
 * 2: FR_ERRORMODE_COMM_HALT, FlexRay controller was stoped due to loss of synchronization.
 * </pre>
 * @param startup_status
 * <pre>
 * 0: FR_STARTUP_UNDEFINED,  Unknown state - controller has not been in the POC:Startup state since POC:Default Config state.
 * 1: FR_STARTUP_COLDSTART_LISTEN, Controller listens before it initiates CAS transmission.
 * 2: FR_STARTUP_INTEGRATION_COLDSTART_CHECK, Node is synchronizing to leading coldstarter.
 * 3: FR_STARTUP_COLDSTART_JOIN, Controller has joined the leading coldstarter in transmission of startup frames.
 * 4: FR_STARTUP_COLDSTART_COLLISION_RESOLUTION,  Leading coldstarter transmits startup frames.
 * 5: FR_STARTUP_COLDSTART_CONSISTENCY_CHECK, Leading coldstarter checks if another node transmits startup frames.
 * 6: FR_STARTUP_INTEGRATION_LISTEN, Integrating node (non-coldstarter) listens.
 * 7: FR_STARTUP_INITIALIZE_SCHEDULE, Schedule is being initialized.
 * 8: FR_STARTUP_INTEGRATION_CONSISTENCY_CHECK,  Controller looks for 4 pairs of startup frames to integrate into cluster.
 * 9: FR_STARTUP_COLDSTART_GAP, Pause between two attempts of coldstart.
 * </pre>
 * @param state
 * <pre>
 * 0: FR_POCSTATE_CONFIG,   POC:Config - controller can be configured
 * 1: FR_POCSTATE_DEFAULT_CONFIG, POC:Default Config - controller is initialized and needs to be configured.
 * 2: FR_POCSTATE_HALT, POC:Halt - controller is stopped
 * 3: FR_POCSTATE_NORMAL_ACTIVE, POC:Normal Active - controller is connected to the FlexRay cluster
 * 4: FR_POCSTATE_NORMAL_PASSIVE, POC:Normal Passive - controller has problems with synchronization to the cluster
 * 5: FR_POCSTATE_READY, POC:Ready - controller is configured and ready to communicate
 * 6: FR_POCSTATE_STARTUP, POC:Startup - controller is starting up the cluster or integrating to the cluster
 * 7: FR_POCSTATE_WAKEUP, POC:Wakeup - controller initiate Wakeup procedure
 * </pre>
 * @return int 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_get_fr_controller_status spxl_get_fr_controller_status
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_get_fr_controller_status(const xl_device_t *device,
                                                      xl_uint8_t *code_start_noise,
                                                      xl_uint8_t *chi_halt_request,
                                                      xl_uint8_t *freeze,
                                                      xl_uint8_t *slot_mode,
                                                      xl_uint8_t *wakeup_status,
                                                      xl_uint8_t *error_mode,
                                                      xl_uint8_t *startup_status,
                                                      xl_uint8_t *state);

#ifndef DISABLE_IO_CONVENTER
int SPXLIBSHARED_EXPORT spxl_set_mp_msg_to_port_configio(const xl_device_t *device,
                                                         xl_uint8_t index,
                                                         xl_mpmsg_iocfg_t *iocfg_msg);

int SPXLIBSHARED_EXPORT spxl_set_mp_msg_to_port_configsig(const xl_device_t *device,
                                                          xl_uint8_t index,
                                                          xl_mpmsg_sigcfg_t *sigcfg_msg);

int SPXLIBSHARED_EXPORT spxl_set_mp_msg_to_port_configmsg(const xl_device_t *device,
                                                          xl_uint8_t index,
                                                          xl_uint8_t protocol,
                                                          xl_uint8_t instance,
                                                          xl_uint32_t id);

//int SPXLIBSHARED_EXPORT spxl_get_mp_msg_to_port_configio(const xl_device_t *device,
//                                                         xl_uint8_t index,
//                                                         xl_mpmsg_iocfg_t *iocfg_msg);

//int SPXLIBSHARED_EXPORT spxl_get_mp_msg_to_port_configsig(const xl_device_t *device,
//                                                          xl_uint8_t index,
//                                                          xl_mpmsg_sigcfg_t *sigcfg_msg);

//int SPXLIBSHARED_EXPORT spxl_get_mp_msg_to_port_configmsg(const xl_device_t *device,
//                                                          xl_uint8_t index,
//                                                          xl_uint8_t *protocol,
//                                                          xl_uint8_t *instance,
//                                                          xl_uint32_t *id);

int SPXLIBSHARED_EXPORT spxl_set_mp_port_to_msg_configio(const xl_device_t *device,xl_uint8_t index,
                                                         uint8_t match,xl_mpmsg_iocfg_t *iocfg_msg);

int SPXLIBSHARED_EXPORT spxl_set_mp_port_to_msg_configsig(const xl_device_t *device,
                                                          xl_uint8_t index,
                                                          xl_mpmsg_sigcfg_t *sigcfg_msg);

int SPXLIBSHARED_EXPORT spxl_set_mp_port_to_msg_configmsg(const xl_device_t *device,
                                                          xl_uint8_t index,
                                                          xl_mpmsg_msgcfg *msgcfg_msg);

int SPXLIBSHARED_EXPORT spxl_set_mp_port_to_msg_configmatch(const xl_device_t *device,
                                                            xl_uint8_t index,
                                                            xl_uint8_t sigIndex,
                                                            xl_uint8_t msgIndex,
                                                            xl_uint8_t factor[8],
                                                            xl_uint8_t offset[8]);


int SPXLIBSHARED_EXPORT spxl_del_mp_msg_to_port_configio(const xl_device_t *device,
                                                          xl_uint8_t index);

int SPXLIBSHARED_EXPORT spxl_del_mp_msg_to_port_configsig(const xl_device_t *device,
                                                         xl_uint8_t index);

int SPXLIBSHARED_EXPORT spxl_del_mp_msg_to_port_configmsg(const xl_device_t *device,
                                                          xl_uint8_t index);

int SPXLIBSHARED_EXPORT spxl_del_mp_port_to_msg_configio(const xl_device_t *device,
                                                          xl_uint8_t index);

int SPXLIBSHARED_EXPORT spxl_del_mp_port_to_msg_configsig(const xl_device_t *device,
                                                         xl_uint8_t index);

int SPXLIBSHARED_EXPORT spxl_del_mp_port_to_msg_configmsg(const xl_device_t *device,
                                                          xl_uint8_t index);

int SPXLIBSHARED_EXPORT spxl_del_mp_port_to_msg_configmatch(const xl_device_t *device,
                                                          xl_uint8_t index);

// int SPXLIBSHARED_EXPORT spxl_get_mp_msg_to_port_configio(const xl_device_t *device,
//                                                           xl_uint8_t index,
//                                                           xl_mpmsg_iocfg_t *mpmsg_io);

// int SPXLIBSHARED_EXPORT spxl_get_mp_msg_to_port_configsig(const xl_device_t *device,
//                                                           xl_uint8_t index,
//                                                           xl_mpmsg_sigcfg_t *mpmsg_sig);

// int SPXLIBSHARED_EXPORT spxl_get_mp_msg_to_port_configmsg(const xl_device_t *device,
//                                                           xl_uint8_t index,
//                                                           xl_mpmsg_msgcfg_t *mpmsg_msg);

// int SPXLIBSHARED_EXPORT spxl_get_mp_port_to_msg_configio(const xl_device_t *device,
//                                                           xl_uint8_t index,
//                                                           xl_mpmsg_iocfg_t *mpmsg_io);

// int SPXLIBSHARED_EXPORT spxl_get_mp_port_to_msg_configsig(const xl_device_t *device,
//                                                           xl_uint8_t index,
//                                                           xl_mpmsg_sigcfg_t *mpmsg_io);

// int SPXLIBSHARED_EXPORT spxl_get_mp_port_to_msg_configmsg(const xl_device_t *device,
//                                                           xl_uint8_t index,
//                                                           xl_mpmsg_msgcfg_t *mpmsg_io);

//int SPXLIBSHARED_EXPORT spxl_get_mp_port_to_msg_configmatch(const xl_device_t *device,
//                                                          xl_uint8_t index,
//                                                            xl_mpmsg_iocfg_t *mpmsg_io);
#endif
/*!
 * spxlibc.h
 * <pre>
 * int spxl_open_blf_log_file(const char *file_path, HANDLE *file);
 * </pre>
 *
 * open a file that you defined the file path;
 *
 * \param file_path The log file path that you want to save log;
 * \param file The Handle pointer to the file that you want to save log;
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_open_blf_log_file spxl_open_blf_log_file
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_open_blf_log_file(const char *file_path, HANDLE &file);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_save_one_lin_frame(const char *file_path, HANDLE *file);
 * </pre>
 *
 * Save one lin frame to a log file
 *
 * \param file The Handle pointer to the file that you save log;
 * \param msg The xl_msg_t pointer to a lin message that you want to save;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_save_one_lin_frame spxl_save_one_lin_frame
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_save_one_lin_frame(HANDLE file, xl_msg_t *msg);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_save_one_fr_frame(const char *file_path, HANDLE *file);
 * </pre>
 *
 * Save one flexray frame to a log file
 *
 * \param file The Handle pointer to the file that you save log;
 * \param msg The xl_msg_t pointer to a flexray message that you want to save;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_save_one_fr_frame spxl_save_one_fr_frame
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_save_one_fr_frame(HANDLE file, xl_msg_t *msg);
/*!
 * spxlibc.h
 * <pre>
 * int spxl_close_blf_log_file(HANDLE *file);
 * </pre>
 *
 * close the blf log file;
 *
 * \param file The Handle pointer to the file that you save log;
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_close_blf_log_file spxl_close_blf_log_file
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_close_blf_log_file(HANDLE file);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_load_fibex(const char* filePath);
 * </pre>
 *
 * load a fibex file;
 *
 * \param filePath The fibex file path,which is an array;
 *
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_load_fibex spxl_load_fibex
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_load_fibex(const char* filePath);

#ifndef FLEXRAY_CMD

/*!
 * spxlibc.h
 * <pre>
 * int spxl_change_fr_signal_raw_value(const xl_device_t *device,
 *                                     const char *frame_name,
 *                                     const char *signal_name,
 *                                     uint64_t raw_value);
 * </pre>
 *
 * change a flexray signal's raw value;
 *
 * \param device The xl_device_t pointer to a specific device;
 * \param frame_name the frame's name which includes the signal that you want to change. which is an array;
 * \param signal_name the signal's name, which is an array;
 * \param raw_value the signal's raw value that you want to change
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_change_fr_signal_raw_value spxl_change_fr_signal_raw_value
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_change_fr_signal_raw_value(const xl_device_t *device, const char *frame_name, const char *signal_name, uint64_t raw_value);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_change_fr_signal_phy_value(const xl_device_t *device,
 *                                     const char *frame_name,
 *                                     const char *signal_name,
 *                                     uint64_t phy_value);
 * </pre>
 *
 * change a flexray signal's phy value;
 *
 * \param device The xl_device_t pointer to a specific device;
 * \param frame_name the frame's name which includes the signal that you want to change. which is an array;
 * \param signal_name the signal's name, which is an array;
 * \param phy_value the signal's phy value that you want to change
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    spxl_change_fr_signal_raw_value(m_device, "CemBackBoneFr03", "AmbTRawQly", 3);
}
</pre>
 * \defgroup spxl_change_fr_signal_phy_value spxl_change_fr_signal_phy_value
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_change_fr_signal_phy_value(const xl_device_t *device, const char *frame_name, const char *signal_name, double phy_value);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_del_fr_command(const xl_device_t *device, const char *frame_name);
 * </pre>
 *
 * delete a flexray frame through the frame's name;
 *
 * \param device The xl_device_t pointer to a specific device;
 * \param frame_name the frame's name which you want to delete, which is an array;
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_send_del_fr_command spxl_send_del_fr_command
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_del_fr_command(const xl_device_t *device, const char *frame_name);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_del_all_fr_command(const xl_device_t *device);
 * </pre>
 *
 * delete all flexray frames;
 *
 * \param device The xl_device_t pointer to a specific device;
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_send_del_all_fr_command spxl_send_del_all_fr_command
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_del_all_fr_command(const xl_device_t *device);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_save_fr_command(const xl_device_t *device, const char *frame_name, int isSave);
 * </pre>
 *
 * save a message to wizbus,which will automatically be sent when power on again;
 *
 * \param device The xl_device_t pointer to a specific device;
 * \param frame_name the frame's name which you want to save. which is an array;
 * \param isSave (bool) 1:true 0:false indicate that whether you want to save it or not;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_send_save_fr_command spxl_send_save_fr_command
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_save_fr_command(const xl_device_t *device, const char *frame_name, int isSave);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_enable_fr_command(const xl_device_t *device, const char *frame_name, int isEnable);
 * </pre>
 *
 * enable or disbale a message;
 *
 * \param device The xl_device_t pointer to a specific device;
 * \param frame_name the frame's name which you want to enable. which is an array;
 * \param isEnable (bool) 1:true 0:false indicate that whether you want to enable it or not;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_send_enable_fr_command spxl_send_enable_fr_command
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_enable_fr_command(const xl_device_t *device, const char *frame_name, int isEnable);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_fr_msg_command(const xl_device_t *device, xl_msg_fr_t *fr_msg);
 * </pre>
 *
 * enable or disbale a message;
 *
 * \param device The xl_device_t pointer to a specific device;
 * \param fr_msg the message which you want to add. which is a structure;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_send_fr_msg_command spxl_send_fr_msg_command
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_fr_msg_command(const xl_device_t *device, xl_msg_fr_t *fr_msg);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_del_fr_msg_command(const xl_device_t *device, xl_msg_fr_t *fr_msg);
 * </pre>
 *
 * delete a message;
 *
 * \param device The xl_device_t pointer to a specific device;
 * \param fr_msg the message which you want to delete. which is a structure;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_send_del_fr_msg_command spxl_send_del_fr_msg_command
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_del_fr_msg_command(const xl_device_t *device, xl_msg_fr_t *fr_msg);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_send_set_fr_command(const xl_device_t *device, const char *frame_name);
 * </pre>
 *
 * send a message to wizbus;
 *
 * \param device The xl_device_t pointer to a specific device;
 * \param frame_name the frame's name which you want to send. which is an array;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{
    int req = spxl_change_fr_signal_raw_value(m_device, "CemBackBoneFr03", "AmbTRawQly", 3);
    if (req == -1) return;
    spxl_send_set_fr_command(m_device, "CemBackBoneFr03");
}
</pre>
 * \defgroup spxl_send_set_fr_command spxl_send_set_fr_command
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_send_set_fr_command(const xl_device_t *device, const char *frame_name);


/*!
 * spxlibc.h
 * <pre>
 * int spxl_get_fr_raw_value_by_frame_name(const xl_device_t *device, const char *frame_name,
                                       const char *signal_name, double &raw_value)
 * </pre>
 *
 * get a signal's raw value by signal name and message name;
 *
 * \param frame_name the frame's name which you want to send. which is an array;
 * \param signal_name the signal's name which you want to send. which is an array;
 * \param data the frame's data;
 * \param raw_value this variable receives the return value;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_get_fr_raw_value_by_frame_name spxl_get_fr_raw_value_by_frame_name
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_get_fr_raw_value_by_frame_name(const char *frame_name, const char *signal_name,
                                                            const uint8_t *data, uint64_t &raw_value);

/*!
 * spxlibc.h
 * <pre>
 * int spxl_get_fr_phy_value_by_frame_name(const xl_device_t *device, const char *frame_name,
                                       const char *signal_name, const uint8_t *data,
                                       double &phy_value)
 * </pre>
 *
 * get a signal's phy value by signal name and message name;
 *
 * \param frame_name the frame's name which you want to send. which is an array;
 * \param signal_name the signal's name which you want to send. which is an array;
 * \param data the frame's data;
 * \param raw_value this variable receives the return value;
 * \return 0 if successful, -1 otherwise.
 *
 * Example usage:
<pre>
void foo( void )
{

}
</pre>
 * \defgroup spxl_get_fr_phy_value_by_frame_name spxl_get_fr_phy_value_by_frame_name
 * \ingroup spxl_ctrl
 */
int SPXLIBSHARED_EXPORT spxl_get_fr_phy_value_by_frame_name(const char *frame_name, const char *signal_name,
                                                            const uint8_t *data, double &phy_value);

int SPXLIBSHARED_EXPORT spxl_get_all_fr_msgs_name_List(char msgsName[][120], int &length);

int SPXLIBSHARED_EXPORT spxl_get_all_fr_signals_name_List(char sigsName[][120], int &length);

int SPXLIBSHARED_EXPORT spxl_get_fr_signals_name_List(const char *frame_name, char sigsName[][120], int &length);

int SPXLIBSHARED_EXPORT spxl_get_fr_msg_info(const char *frame_name, FibexFrameInfo &msgInfo);

int SPXLIBSHARED_EXPORT spxl_get_fr_signal_info(const char *signal_name, FibexSignalInfo &sigInfo);

int SPXLIBSHARED_EXPORT spxl_launch_iEngine(const char *iEnginePath);

//int SPXLIBSHARED_EXPORT spxl_get_fr_nodes(char nodesName[][20]);

#endif

int SPXLIBSHARED_EXPORT spxl_save_all_configurations(const xl_device *device);

int SPXLIBSHARED_EXPORT spxl_restore_to_default(const xl_device *device);
int SPXLIBSHARED_EXPORT spxl_save_all_configurations(const xl_device *device);

int SPXLIBSHARED_EXPORT spxl_restore_to_default(const xl_device *device);

#ifdef __cplusplus
} // extern "C"
#endif

#endif // _SPXLAPI_H_
