#include "FXClusterType.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXClusterType::FXClusterType(FXFibex *fibex)
    : FXRevisedElementType()
    , m_fibex(fibex)
{
}

FXClusterType::~FXClusterType()
{
    if (m_protocolVersion) delete m_protocolVersion;
    if (m_physical) delete m_physical;
    if (m_physicalVersion) delete m_physicalVersion;
    m_channelRefList.clear();
    if (m_medium) delete m_medium;
    if (m_numberOfCycles) delete m_numberOfCycles;
    if (m_maxFrameLength) delete m_maxFrameLength;
    if (m_canFdSpeed) delete m_canFdSpeed;
}

//
void FXClusterType::load(const QDomElement &element)
{
    FXRevisedElementType::load(element);

    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXClusterType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:SPEED") {
            m_speed = childElement.text().toULongLong();
        } else if (childElement.tagName() == "fx:IS-HIGH-LOW-BIT-ORDER") {
            m_isHighLowBitOrder = QVariant(childElement.text()).toBool();
        } else if (childElement.tagName() == "fx:BIT-COUNTING-POLICY") {
            QString typeStr = childElement.text();
            FibexTypes::EnumParser<FibexTypes::FXBitCountingPolicy> ep;
            FibexTypes::FXBitCountingPolicy type;
            bool isOk = ep.str2Enum(typeStr, type);
            if (isOk) {
                m_bitCountingPolicy = type;
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXClusterType::load, m_bitCountingPolicy =" << typeStr << (int)m_bitCountingPolicy;
#endif
            }
        } else if (childElement.tagName() == "fx:PROTOCOL") {
            m_protocol = childElement.text();
        } else if (childElement.tagName() == "fx:PROTOCOL-VERSION") {
            if (!m_protocolVersion) {
                m_protocolVersion = new QString();
                *m_protocolVersion = childElement.text();
            }
        } else if (childElement.tagName() == "fx:PHYSICAL") {
            if (!m_physical) {
                m_physical = new QString();
                *m_physical = childElement.text();
            }
        } else if (childElement.tagName() == "fx:PHYSICAL-VERSION") {
            if (!m_physicalVersion) {
                m_physicalVersion = new QString();
                *m_physicalVersion = childElement.text();
            }
        } else if (childElement.tagName() == "fx:CHANNEL-REFS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXClusterType::load CHANNEL-REFS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:CHANNEL-REF") {
                    QString idRef = subChildElement.attribute("ID-REF");
                    m_channelRefList.append(idRef);
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:MEDIUM") {
            if (!m_medium) {
                m_medium = new FibexTypes::FXMedium();
                QString typeStr = childElement.text();
                FibexTypes::EnumParser<FibexTypes::FXMedium> ep;
                FibexTypes::FXMedium type;
                bool isOk = ep.str2Enum(typeStr, type);
                if (isOk) {
                    *m_medium = type;
    #ifndef F_NO_DEBUG
                    QLOG_TRACE() << "FXClusterType::load, m_medium =" << typeStr << (int)(*m_medium);
    #endif
                }
            }
        } else if (childElement.tagName() == "fx:NUMBER-OF-CYCLES") {
            if (!m_numberOfCycles) {
                m_numberOfCycles = new qint16();
                *m_numberOfCycles = childElement.text().toShort();
            }
        } else if (childElement.tagName() == "fx:MAX-FRAME-LENGTH") {
            if (!m_maxFrameLength) {
                m_maxFrameLength = new quint32();
                *m_maxFrameLength = childElement.text().toUInt();
            }
        } else if (childElement.tagName() == "fx:CAN-FD-SPEED") {
            if (!m_canFdSpeed) {
                m_canFdSpeed = new quint64();
                *m_canFdSpeed = childElement.text().toULongLong();
            }
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
