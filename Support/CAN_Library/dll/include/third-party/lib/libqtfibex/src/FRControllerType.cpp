#include "FRControllerType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FRControllerType::FRControllerType(FXFibex *fibex)
    : FXControllerType(fibex)
    , m_fibex(fibex)
{
}

void FRControllerType::load(const QDomElement &element)
{
    FXControllerType::load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FRControllerType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "flexray:KEY-SLOT-USAGE") {
            m_keySlotUsage.load(childElement);
        } else if (childElement.tagName() == "flexray:MAX-DYNAMIC-PAYLOAD-LENGTH") {
            m_maxDynamicPayloadLength = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_maxDynamicPayloadLength =" << m_maxDynamicPayloadLength;
#endif
        } else if (childElement.tagName() == "flexray:MAX-DYNAMIC-PAYLOAD-LENGTH") {
            m_maxDynamicPayloadLength = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_maxDynamicPayloadLength =" << m_maxDynamicPayloadLength;
#endif
        } else if (childElement.tagName() == "flexray:CLUSTER-DRIFT-DAMPING") {
            m_clusterDriftDamping = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_clusterDriftDamping =" << m_clusterDriftDamping;
#endif
        } else if (childElement.tagName() == "flexray:DECODING-CORRECTION") {
            m_decodingCorrection = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_decodingCorrection =" << m_decodingCorrection;
#endif
        } else if (childElement.tagName() == "flexray:LISTEN-TIMEOUT") {
            m_listenTimeout = childElement.text().toUInt();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_listenTimeout =" << m_listenTimeout;
#endif
        } else if (childElement.tagName() == "flexray:MAX-DRIFT") {
            m_maxDrift = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_maxDrift =" << m_maxDrift;
#endif
        } else if (childElement.tagName() == "flexray:EXTERN-OFFSET-CORRECTION") {
            m_externOffsetCorrection = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_externOffsetCorrection =" << m_externOffsetCorrection;
#endif
        } else if (childElement.tagName() == "flexray:EXTERN-RATE-CORRECTION") {
            m_externRateCorrection = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_externRateCorrection =" << m_externRateCorrection;
#endif
        } else if (childElement.tagName() == "flexray:LATEST-TX") {
            m_latestTx = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_latestTx =" << m_latestTx;
#endif
        } else if (childElement.tagName() == "flexray:MICRO-PER-CYCLE") {
            m_microPerCycle = childElement.text().toUInt();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_microPerCycle =" << m_microPerCycle;
#endif
        } else if (childElement.tagName() == "flexray:OFFSET-CORRECTION-OUT") {
            m_offsetCorrectionOut = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_offsetCorrectionOut =" << m_offsetCorrectionOut;
#endif
        } else if (childElement.tagName() == "flexray:RATE-CORRECTION-OUT") {
            m_rateCorrectionOut = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_rateCorrectionOut =" << m_rateCorrectionOut;
#endif
        } else if (childElement.tagName() == "flexray:SAMPLES-PER-MICROTICK") {
            m_samplesPerMicrotick = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_samplesPerMicrotick =" << m_samplesPerMicrotick;
#endif
        } else if (childElement.tagName() == "flexray:DELAY-COMPENSATION-A") {
            m_delayCompensationA = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_delayCompensationA =" << m_delayCompensationA;
#endif
        } else if (childElement.tagName() == "flexray:DELAY-COMPENSATION-B") {
            m_delayCompensationB = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_delayCompensationB =" << m_delayCompensationB;
#endif
        } else if (childElement.tagName() == "flexray:WAKE-UP-PATTERN") {
            m_wakeUpPattern = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_wakeUpPattern =" << m_wakeUpPattern;
#endif
        } else if (childElement.tagName() == "flexray:ALLOW-HALT-DUE-TO-CLOCK") {
            m_allowHaltDueToClock = QVariant(childElement.text()).toBool();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_allowHaltDueToClock =" << m_allowHaltDueToClock;
#endif
        } else if (childElement.tagName() == "flexray:ALLOW-PASSIVE-TO-ACTIVE") {
            m_allowPassiveToActive = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_allowPassiveToActive =" << m_allowPassiveToActive;
#endif
        } else if (childElement.tagName() == "flexray:ACCEPTED-STARTUP-RANGE") {
            m_acceptedStartupRange = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_acceptedStartupRange =" << m_acceptedStartupRange;
#endif
        } else if (childElement.tagName() == "flexray:MACRO-INITIAL-OFFSET-A") {
            m_macroInitialOffsetA = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_macroInitialOffsetA =" << m_macroInitialOffsetA;
#endif
        } else if (childElement.tagName() == "flexray:MACRO-INITIAL-OFFSET-B") {
            m_macroInitialOffsetB = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_macroInitialOffsetB =" << m_macroInitialOffsetB;
#endif
        } else if (childElement.tagName() == "flexray:MICRO-INITIAL-OFFSET-A") {
            m_microInitialOffsetA = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_microInitialOffsetA =" << m_microInitialOffsetA;
#endif
        } else if (childElement.tagName() == "flexray:MICRO-INITIAL-OFFSET-B") {
            m_microInitialOffsetB = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_microInitialOffsetB =" << m_microInitialOffsetB;
#endif
        } else if (childElement.tagName() == "flexray:SINGLE-SLOT-ENABLED") {
            m_singleSlotEnabled = QVariant(childElement.text()).toBool();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_singleSlotEnabled =" << m_singleSlotEnabled;
#endif
        } else if (childElement.tagName() == "flexray:MICROTICK") {
            m_microtick = childElement.text().toFloat();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_microtick =" << m_microtick;
#endif
        } else if (childElement.tagName() == "flexray:MICRO-PER-MACRO-NOM") {
            m_microPerMacroNom = childElement.text().toFloat();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FRControllerType::load, m_microPerMacroNom =" << m_microPerMacroNom;
#endif
        }

        child = child.nextSibling();
    }


}

} // FIBEX
} // ASAM
