#include "FRClusterType.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FRClusterType::FRClusterType(FXFibex *fibex)
    : FXClusterType(fibex)
{
}

FRClusterType::~FRClusterType()
{
    if (m_wakeUp) delete m_wakeUp;
}

void FRClusterType::load(const QDomElement &element)
{
    FXClusterType::load(element);

    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FRClusterType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "flexray:COLD-START-ATTEMPTS") {
            m_coldStartAttempts = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:ACTION-POINT-OFFSET") {
            m_actionPointOffset = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:DYNAMIC-SLOT-IDLE-PHASE") {
            m_dynamicSlotIdlePhase = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:MINISLOT") {
            m_miniSlot = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:MINISLOT-ACTION-POINT-OFFSET") {
            m_minislotActionPointOffset = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:N-I-T") {
            m_NIT = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:SAMPLE-CLOCK-PERIOD") {
            m_sampleClockPeriod = childElement.text().toFloat();
        } else if (childElement.tagName() == "flexray:STATIC-SLOT") {
            m_staticSlot = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:SYMBOL-WINDOW") {
            m_symbolWindow = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:T-S-S-TRANSMITTER") {
            m_TSSTransmitter = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:WAKE-UP") {
            if (!m_wakeUp) {
                m_wakeUp = new FRWakeUp();
                m_wakeUp->load(childElement);
            }
        } else if (childElement.tagName() == "flexray:LISTEN-NOISE") {
            m_listenNoise = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:MACRO-PER-CYCLE") {
            m_macroPerCycle = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:MACROTICK") {
            m_macroTick = childElement.text().toFloat();
        } else if (childElement.tagName() == "flexray:MAX-INITIALIZATION-ERROR") {
            m_maxInitializationError = childElement.text().toFloat();
        } else if (childElement.tagName() == "flexray:MAX-WITHOUT-CLOCK-CORRECTION-FATAL") {
            m_maxWithoutClockCorrectionFatal = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:MAX-WITHOUT-CLOCK-CORRECTION-PASSIVE") {
            m_maxWithoutClockCorrectionPassive = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:NETWORK-MANAGEMENT-VECTOR-LENGTH") {
            m_networkManagementVectorLength = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:NUMBER-OF-MINISLOTS") {
            m_numberOfMinislots = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:NUMBER-OF-STATIC-SLOTS") {
            m_numberOfStaticSlots = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:OFFSET-CORRECTION-START") {
            m_offsetCorrectionStart = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:PAYLOAD-LENGTH-STATIC") {
            m_payloadLengthStatic = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:SYNC-NODE-MAX") {
            m_syncNodeMax = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:CAS-RX-LOW-MAX") {
            m_casRxLowMax = childElement.text().toUInt();
        } else if (childElement.tagName() == "flexray:BIT") {
            m_bit = childElement.text().toFloat();
        } else if (childElement.tagName() == "flexray:CYCLE") {
            m_cycle = childElement.text().toUShort();
        } else if (childElement.tagName() == "flexray:CLUSTER-DRIFT-DAMPING") {
            m_clusterDriftDamping = childElement.text().toUInt();
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
