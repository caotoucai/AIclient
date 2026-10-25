#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXClusterType.h"
#include "FRWakeUp.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType CLUSTER-TYPE
 */
class ASAM_FIBEX_EXPORT FRClusterType : public FXClusterType
{
public:
    FRClusterType(FXFibex *fibex);
    ~FRClusterType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element COLD-START-ATTEMPTS */
    quint8 m_coldStartAttempts = 16;
    /** @todo element ACTION-POINT-OFFSET */
    quint8 m_actionPointOffset = 32;
    /** @todo element DYNAMIC-SLOT-IDLE-PHASE */
    quint8 m_dynamicSlotIdlePhase = 0;
    /** @todo element MINISLOT */
    quint8 m_miniSlot = 16;
    /** @todo element MINISLOT-ACTION-POINT-OFFSET */
    quint8 m_minislotActionPointOffset = 16;
    /** @todo element N-I-T */
    quint16 m_NIT = 256;
    /** @todo element SAMPLE-CLOCK-PERIOD */
    float m_sampleClockPeriod;
    /** @todo element STATIC-SLOT */
    quint16 m_staticSlot = 512;
    /** @todo element SYMBOL-WINDOW */
    quint8 m_symbolWindow = 1;
    /** @todo element T-S-S-TRANSMITTER */
    quint8 m_TSSTransmitter = 5;
    /** @todo element WAKE-UP */
    FRWakeUp *m_wakeUp = nullptr;
    /** @todo element LISTEN-NOISE */
    quint8 m_listenNoise = 8;
    /** @todo element MACRO-PER-CYCLE */
    quint16 m_macroPerCycle = 8000;
    /** @todo element MACROTICK */
    float m_macroTick = 1.0;
    /** @todo element MAX-INITIALIZATION-ERROR */
    float m_maxInitializationError = 0.0;
    /** @todo element MAX-WITHOUT-CLOCK-CORRECTION-FATAL */
    quint8 m_maxWithoutClockCorrectionFatal = 1;
    /** @todo element MAX-WITHOUT-CLOCK-CORRECTION-PASSIVE */
    quint8 m_maxWithoutClockCorrectionPassive = 1;
    /** @todo element NETWORK-MANAGEMENT-VECTOR-LENGTH */
    quint8 m_networkManagementVectorLength = 0;
    /** @todo element NUMBER-OF-MINISLOTS */
    quint16 m_numberOfMinislots = 4096;
    /** @todo element NUMBER-OF-STATIC-SLOTS */
    quint16 m_numberOfStaticSlots = 512;
    /** @todo element OFFSET-CORRECTION-START */
    quint16 m_offsetCorrectionStart = 8192;
    /** @todo element PAYLOAD-LENGTH-STATIC */
    quint8 m_payloadLengthStatic = 64;
    /** @todo element SYNC-NODE-MAX */
    quint8 m_syncNodeMax = 8;
    /** @todo element CAS-RX-LOW-MAX */
    quint8 m_casRxLowMax;
    /** @todo element BIT */
    float m_bit;
    /** @todo element CYCLE */
    quint16 m_cycle = 8192;
    /** @todo element CLUSTER-DRIFT-DAMPING */
    quint8 m_clusterDriftDamping = 0;
};

} // FIBEX
} // ASAM
