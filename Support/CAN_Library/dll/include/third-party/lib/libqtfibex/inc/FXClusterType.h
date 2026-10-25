#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"
#include "FXRevisedElementType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType fx:CLUSTER-TYPE
 *
 * Content model for the entity CLUSTER with platform specific extendability.
 */
class ASAM_FIBEX_EXPORT FXClusterType : public FXRevisedElementType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXClusterType(FXFibex *fibex);
    ~FXClusterType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @required element SPEED */
    quint64 m_speed = 10000000;
    /** @required element IS-HIGH-LOW-BIT-ORDER */
    bool m_isHighLowBitOrder;
    /** @required element BIT-COUNTING-POLICY */
    FibexTypes::FXBitCountingPolicy m_bitCountingPolicy;
    /** @required element PROTOCOL */
    QString m_protocol = "CAN";
    /** @opt element PROTOCOL-VERSION */
    QString *m_protocolVersion = nullptr;
    /** @opt element PHYSICAL */
    QString *m_physical = nullptr;
    /** @opt element PHYSICAL-VERSION */
    QString *m_physicalVersion = nullptr;
    /** @opt element CHANNEL-REFS */
    QList<QString> m_channelRefList;
    /** @opt element MEDIUM */
    FibexTypes::FXMedium *m_medium = nullptr;
    /** @opt element NUMBER-OF-CYCLES */
    qint16 *m_numberOfCycles = nullptr;
    /** @opt element MAX-FRAME-LENGTH */
    quint32 *m_maxFrameLength = nullptr;
    /** @opt element fx:CAN-FD-SPEED */
    quint64 *m_canFdSpeed = nullptr;
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
