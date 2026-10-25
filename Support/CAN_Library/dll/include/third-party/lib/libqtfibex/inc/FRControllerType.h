#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"
#include "FXControllerType.h"
#include "FRKeySlotUsage.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType CONTROLLER-TYPE
 *
 * Content model for the entity CONTROLLER with platform specific extendability.
 */
class ASAM_FIBEX_EXPORT FRControllerType : public FXControllerType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FRControllerType(FXFibex *fibex = nullptr);

    /** load from XML DOM element */
    void load(const QDomElement &element);

    FRKeySlotUsage m_keySlotUsage;

    /**
     * @brief MAX-DYNAMIC-PAYLOAD-LENGTH
     *
     * Maximum payload length of a dynamic frame in 16bit WORDS (pPayloadLengthDynMax)
     */
    quint8 m_maxDynamicPayloadLength;

    /**
     * @brief CLUSTER-DRIFT-DAMPING
     *
     * Local cluster drift damping factor used for rate correction. Unit:µT (pClusterDriftDamping)
     */
    quint8 m_clusterDriftDamping;

    /**
     * @brief DECODING-CORRECTION
     *
     * Value used by the receiver to calculate the difference between primary time reference point and secondary time reference point.
     * Unit: µT (pDecodingCorrection)
     */
    quint8 m_decodingCorrection;

    /**
     * @brief LISTEN-TIMEOUT
     *
     * Upper limit for the start up listen timeout and wake up listen timeout.
     * pdListenTimeout[µT] = 2 * (pMicroPerCycle[µT] + pdMaxDrift[µT]).
     * Unit:µT (pdListenTimeout)
     */
    quint32 m_listenTimeout;

    /**
     * @brief MAX-DRIFT
     *
     * Maximum drift offset between two nodes that operate with unsynchronized clocks over one communication cycle.
     * Unit: µT (pdMaxDrift)
     */
    quint16 m_maxDrift;

    /**
     * @brief EXTERN-OFFSET-CORRECTION
     *
     * Number of microticks added or subtracted to the NIT to carry out a host-requested external offset correction.
     * (pExternOffsetCorrection)
     */
    quint8 m_externOffsetCorrection;

    /**
     * @brief EXTERN-RATE-CORRECTION
     *
     * Number of microticks added or subtracted to the cycle to carry out a host-requested external rate correction.
     * (pExternRateCorrection)
     */
    quint8 m_externRateCorrection;

    /**
     * @brief LATEST-TX
     *
     * Number of the last minislot in which a frame transmission can start in the dynamic segment.
     * Unit:minislot
     * (pLatestTx)
     */
    quint16 m_latestTx;

    /**
     * @brief MICRO-PER-CYCLE
     *
     * Nominal number of microticks in the communication cycle of the local node.
     * If nodes have different microtick durations this number will differ from node to node.
     * (pMicroPerCycle)
     */
    quint32 m_microPerCycle;

    /**
     * @brief OFFSET-CORRECTION-OUT
     *
     * Magnitude of the maximum permissible offset correction value.
     * Unit:microtick
     * (pOffsetCorrectionOut)
     */
    quint16 m_offsetCorrectionOut;

    /**
     * @brief RATE-CORRECTION-OUT
     *
     * Magnitude of the maximum permissible rate correction value.
     * Unit:µT
     * (pRateCorrectionOut)
     */
    quint16 m_rateCorrectionOut;

    /**
     * @brief SAMPLES-PER-MICROTICK
     *
     * Number of samples per microtick (Shall not be part of the conformance test due to implementation dependency).
     * pSamplesPerMicrotick = pdMicrotick / gdSampleClockPeriod = pdMicrotick / (gdBit / cSamplesPerBit) = 8 * pdMicrotick / gdBit;
     * (pSamplesPerMicrotick)
     */
    quint8 m_samplesPerMicrotick;

    /**
     * @brief DELAY-COMPENSATION-A
     *
     * Value used to compensate for reception delays on channel A.
     * This covers assumed propagation delay up to cPropagationDelayMax for microticks in the range of 0.0125 µs to 0.05 µs.
     * In practice, the minimum of the propagation delays of all sync nodes should be applied.
     * Unit:microticks
     * Unit:µT
     * (pDelayCompensation[A])
     */
    quint8 m_delayCompensationA;

    /**
     * @brief DELAY-COMPENSATION-B
     *
     * Value used to compensate for reception delays on channel B.
     * This covers assumed propagation delay up to cPropagationDelayMax for microticks in the range of 0.0125 µs to 0.05 µs.
     * In practice, the minimum of the propagation delays of all sync nodes should be applied.
     * Unit:microticks
     * Unit:µT
     * (pDelayCompensation[B])
     */
    quint8 m_delayCompensationB;

    /**
     * @brief WAKE-UP-PATTERN
     *
     * Number of repetitions of the wakeup symbol that are combined to form a wakeup pattern when the node enters the POC:wakeup send state.
     * (pWakeupPattern)
     */
    quint8 m_wakeUpPattern;

    /**
     * @brief ALLOW-HALT-DUE-TO-CLOCK
     *
     * Boolean flag that controls the transition to the POC:halt state due to a clock synchronization errors If set to true, the CC is allowed to transition to POC:halt.
     * If set to false, the CC will not transition to the POC:halt state but will enter or remain in the POC:normal passive state (self healing would still be possible).
     * (pAllowHaltDueToClock)
     */
    bool m_allowHaltDueToClock;

    /**
     * @brief ALLOW-PASSIVE-TO-ACTIVE
     *
     * Number of consecutive even/odd cycle pairs that must have valid clock correction terms before the CC will be allowed to transition from the POC:normal passive state to POC:normal active state.
     * If set to 0, the CC is not allowed to transition from POC:normal passive to POC:normal active.
     * (pAllowPassiveToActive)
     */
    quint8 m_allowPassiveToActive;

    /**
     * @brief ACCEPTED-STARTUP-RANGE
     *
     * Expanded range of measured clock deviation allowed for startup frames during integration.
     * Unit:µT
     * (pdAcceptedStartupRange)
     */
    quint16 m_acceptedStartupRange;

    /**
     * @brief MACRO-INITIAL-OFFSET-A
     *
     * Integer number of macroticks between the static slot boundary and the closest macrotick boundary of the secondary time reference point based on the nominal macrotick duration.
     * (pMacroInitialOffset)
     */
    quint8 m_macroInitialOffsetA;

    /**
     * @brief MACRO-INITIAL-OFFSET-B
     *
     * Integer number of macroticks between the static slot boundary and the closest macrotick boundary of the secondary time reference point based on the nominal macrotick duration.
     * (pMacroInitialOffset)
     */
    quint8 m_macroInitialOffsetB;

    /**
     * @brief MICRO-INITIAL-OFFSET-A
     *
     * Number of microticks between the closest macrotick boundary described by gMacroInitialOffset and the secondary time reference point.
     * The parameter depends on pDelayCompensation[x] and therefor it has to be set independently for each channel.
     * (pMicroInitialOffset[A])
     */
    quint8 m_microInitialOffsetA;

    /**
     * @brief MICRO-INITIAL-OFFSET-B
     *
     * Number of microticks between the closest macrotick boundary described by gMacroInitialOffset and the secondary time reference point.
     * The parameter depends on pDelayCompensation[x] and therefor it has to be set independently for each channel.
     * (pMicroInitialOffset[B])
     */
    quint8 m_microInitialOffsetB;

    /**
     * @brief SINGLE-SLOT-ENABLED
     *
     * Flag indicating whether or not the node shall enter single slot mode following startup.
     * (pSingleSlotEnabled)
     */
    bool m_singleSlotEnabled;

    /** MICROTICK */
    float m_microtick;

    /** MICRO-PER-MACRO-NOM */
    float m_microPerMacroNom;
};

} // FIBEX
} // ASAM
