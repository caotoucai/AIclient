#ifndef ICOMMDEFINE_H
#define ICOMMDEFINE_H

#if 0
// protocol v1 ---------------------------------------------
#define PROTOCOL_ID_CONFIGURATION  	0x08
#define PROTOCOL_ID_KEYWORD_82     	0x10
#define PROTOCOL_ID_KEYWORD_71     	0x18
#define PROTOCOL_ID_IIC            	0x20
#define PROTOCOL_ID_KEYWORD_2K     	0x28
#define PROTOCOL_ID_IDB            	0x30
#define PROTOCOL_ID_ACP            	0x38
#define PROTOCOL_ID_EC             	0x40
#define PROTOCOL_ID_J1708          	0x48
#define PROTOCOL_ID_CAN1           	0x50
#define PROTOCOL_ID_CAN2           	0x58
#define PROTOCOL_ID_CLASS2         	0x60
#define PROTOCOL_ID_AOS            	0x68
#define PROTOCOL_ID_SPI            	0x70
#define PROTOCOL_ID_UPGRADE             0x78
#define PROTOCOL_ID_UART           	0x80
#define PROTOCOL_ID_J1850          	0x88
#define PROTOCOL_ID_BEAN1          	0x90
#define PROTOCOL_ID_BEAN2          	0x98
#define PROTOCOL_ID_LIN1         	0xB0
#define PROTOCOL_ID_LIN2                0xB8
#define PROTOCOL_ID_FLEXRAY             0xE0
#define PROTOCOL_ID_ISO15765_CAN1  	0xC0
#define PROTOCOL_ID_ISO15765_CAN2  	0xC8
#define PROTOCOL_ID_BLOCK_TRANSFER 	0xF8
#define PROTOCOL_ID_BIT_FD              0x80
#define PROTOCOL_ID_CAN1_FD             0xD0
#define PROTOCOL_ID_CAN2_FD             0xD8
#define PROTOCOL_ID_BIT_BUS             0x08

enum BUS_TYPE {
    BUS_INVALID                         = -1,
    BUS_START_INDEX                     = 0,
    BUS_PHY_START_INDEX                 = BUS_START_INDEX,
    BUS_CAN_START                       = BUS_START_INDEX,
    BUS_CAN1                            = BUS_CAN_START,
    BUS_CAN2,
    BUS_CAN_END                         = BUS_CAN2,
    BUS_LIN_START,
    BUS_LIN1                            = BUS_LIN_START,
    BUS_LIN2,
    BUS_LIN_END                         = BUS_LIN2,
    BUS_PHY_END_INDEX,
    BUS_CANFD_START                     = BUS_PHY_END_INDEX,
    BUS_CANFD1                          = BUS_CANFD_START,
    BUS_CANFD2,
    BUS_CANFD_END                       = BUS_CANFD2,
    BUS_ISOTP_START,
    BUS_ISOTP1                          = BUS_ISOTP_START,
    BUS_ISOTP2,
    BUS_ISOTP_END                       = BUS_ISOTP2,
    BUS_END_INDEX,
    BUS_NUMBER                          = BUS_END_INDEX
};

#define IS_BUS_CAN(id)                  ((id >= BUS_CAN_START) && (id <= BUS_CAN_END))
#define IS_BUS_LIN(id)                  ((id >= BUS_LIN_START) && (id <= BUS_LIN_END))
#define IS_BUS_VALID(id)                ((id >= BUS_START_INDEX) && (id <= BUS_END_INDEX))
#define BUS_CAN_NUMBER                  (BUS_CAN_END-BUS_CAN_START+1)
#define BUS_LIN_NUMBER                  (BUS_LIN_END-BUS_LIN_START+1)
#define BUS_PHY_NUMBER                  (BUS_PHY_END_INDEX-BUS_PHY_START_INDEX)

#define PROTOCOL_ID_MASK                0xF8
#define PROTOCOL_ID_CAN_MASK            0x78
#define COMMAND_MASK                    0x04
#define TXRX_MASK                       0x02
#define TIMESTAMP_MASK                  0x01
#define PROTOCOL_IDCMD_MASK             (PROTOCOL_ID_MASK | COMMAND_MASK)

// protocol v2 ---------------------------------------------
#define PROTOCOL_ID_MAGIC               0xA0
#define PROTOCOL_CATEGORY_CAN           0x00
#define PROTOCOL_CATEGORY_CANFD         0x01
#define PROTOCOL_CATEGORY_ISO15765_2    0x02
#define PROTOCOL_CATEGORY_LIN           0x03
#define PROTOCOL_CATEGORY_ETH           0x04
#define PROTOCOL_CATEGORY_FR            0x05
#define PROTOCOL_CATEGORY_GBLSETTINGS   0x1F

#define PROTOCOL_BIT_COMMAND            7
#define PROTOCOL_BIT_TXRX               6
#define PROTOCOL_BIT_EXTHDR             5

#define PROTOCOL_MASK_MAGIC             PROTOCOL_ID_MAGIC
#define PROTOCOL_MASK_CATEGORY          0x1F
#define PROTOCOL_MASK_COMMAND           0x80
#define PROTOCOL_MASK_TXRX              0x40
#define PROTOCOL_MASK_EXTHDR            0x20
#define PROTOCOL_MASK_INSTANCE          0x1F

#endif

// common ---------------------------------------------
#define LIN_VER_2_0                     0x20
#define LIN_VER_1_3                     0x13

#define LIN_CHECKSUM_LEN                1
#define COMPLETE_CODE_LEN               1
#define MSG_END_STR_LEN                 2

#define DIRECTION_RX                    0
#define DIRECTION_TX                    1

#define IS_BIT_SET(data, bit)           (((data)>>(bit))&0x1)
#define SET_BIT(data, bit)              (data) = (data)|(1<<(bit))
#define CLEAR_BIT(data, bit)            (data) = (data)&(~(1<<(bit)))

#define EXTENTED_ID_BIT                 31
#define EXTENTED_ID_MASK                0x7FFFFFFFUL
#define IS_EXTENDED(id)                 ((id & 0x1FFFF800U)?1:0)  //((id > 0x7FF)?1:0)

#define ARRAY_SIZE(x)                   sizeof((x))/sizeof((x)[0])

#endif // ICOMMDEFINE_H
