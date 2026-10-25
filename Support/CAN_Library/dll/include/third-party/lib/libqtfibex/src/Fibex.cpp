#include "../inc/Fibex.h"
#include "../inc/LogDb.h"

namespace ASAM {
namespace FIBEX {

Fibex::Fibex()
    : FXFibex()
    , m_domElement(nullptr)
{

}

Fibex::~Fibex()
{
    qDeleteAll(m_ecus);
    m_ecus.clear();

    qDeleteAll(m_channels);
    m_channels.clear();

    qDeleteAll(m_frames);
    m_frames.clear();

    qDeleteAll(m_pdus);
    m_pdus.clear();

    qDeleteAll(m_signals);
    m_signals.clear();
}

bool Fibex::covertXml2Db()
{    
#ifndef F_NO_DEBUG
    QLOG_DEBUG() << "covertXml2Db --------------------------------------------";
#endif

    {
        QHash<QString, FRClusterType*> &clusters = m_elements->m_clusters;
        QHash<QString, FRClusterType*>::const_iterator ci;
        for (ci = clusters.constBegin(); ci != clusters.constEnd(); ci++) {
            FBCluster *fbCluster = new FBCluster();
            const FRClusterType *frCluster = ci.value();
            fbCluster->m_frCluster = frCluster;
            fbCluster->m_msPerCycle = static_cast<quint32>(frCluster->m_macroTick)
                    * static_cast<quint32>(frCluster->m_macroPerCycle) / 1000;
            m_clusters[frCluster->m_id] = fbCluster;
        }
    }

    {
        QHash<QString, FXSignalType*> &sigs = m_elements->m_signals;

        for (auto ci = sigs.constBegin(); ci != sigs.constEnd(); ci++) {
            FBSignal *fbSignal = new FBSignal();
            m_signals[ci.key()] = fbSignal;
            const FXSignalType *fxSignal = ci.value();

            fbSignal->m_fxSignal = fxSignal;

            m_signalNameMap.insert(fxSignal->m_shortName, fbSignal);

#ifndef F_NO_DEBUG
            //QLOG_DEBUG() << ci.key() << fxSignal->m_shortName << fxSignal->m_codingRef;
#endif
            if (fxSignal->m_sigType) {
                fbSignal->m_signalType = &fxSignal->m_sigType->m_type;
            }

            if (!m_processingInformation->m_codings) {
                continue;
            }

            QHash<QString, CodingType*> &codings = m_processingInformation->m_codings->m_codings;

            if (!fxSignal->m_codingRef.isEmpty()
                    && codings.contains(fxSignal->m_codingRef)) {
                CodingType *ct = codings.value(fxSignal->m_codingRef);
#ifndef F_NO_DEBUG
                //QLOG_DEBUG() << "found codingType" << ct->m_shortName;
#endif

                if (ct->m_codedType) {
                    fbSignal->m_bitSize = ct->m_codedType->m_bitLength;
                    if (ct->m_codedType->m_baseDataType) {
                        fbSignal->m_codedType = *ct->m_codedType->m_baseDataType;
                    }
                }

                if (ct->m_compuMethods
                        && ct->m_compuMethods->m_compuMethods.count() > 0) {
                    HOCompuMethod *cm = ct->m_compuMethods->m_compuMethods.at(0);
#ifndef F_NO_DEBUG
                    //QLOG_DEBUG() << QObject::tr("m_compuCategory = %1").arg((int)cm->m_compuCategory);
#endif
                    if (cm->m_unitRef
                            && (m_processingInformation->m_codings)) {
                        QHash<QString, HOUnit*> &units = m_processingInformation->m_unitSpec->m_units;
                        if (units.contains(*cm->m_unitRef)) {
                            HOUnit *unit = units.value(*cm->m_unitRef);
                            if (unit->m_displayName) {
                                fbSignal->m_unit = *unit->m_displayName;
                            }
                        }
                    }

                    if (cm->m_physConstrs) {
                        if (cm->m_physConstrs->m_scaleConstr->m_lowerLimit) {
                            fbSignal->m_minimumPhysicalValue = *cm->m_physConstrs->m_scaleConstr->m_lowerLimit;
                        }
                        if (cm->m_physConstrs->m_scaleConstr->m_upperLimit) {
                            fbSignal->m_maximumPhysicalValue = *cm->m_physConstrs->m_scaleConstr->m_upperLimit;
                        }
                    }

                    switch (cm->m_compuCategory) {
                        case FibexTypes::HOCompuCategorySt::LINEAR: {
                            if (cm->m_compuInternalToPhys
                                    && (cm->m_compuInternalToPhys->m_compuScales.count() > 0)
                                    && cm->m_compuInternalToPhys->m_compuScales.at(0)->m_compuRationalCoeffs) {
                                HOCompuRationalCoeffs *crf = cm->m_compuInternalToPhys->m_compuScales.at(0)->m_compuRationalCoeffs;
                                if ((crf->m_compuNumeratorList.count() >= 2)
                                        && crf->m_compuDenominatorList.count() > 0) {
                                    fbSignal->m_factor = crf->m_compuNumeratorList.at(1)/crf->m_compuDenominatorList.at(0);
                                    fbSignal->m_offset = crf->m_compuNumeratorList.at(0)/crf->m_compuDenominatorList.at(0);
                                }
                            }
                            break;
                        }
                        case FibexTypes::HOCompuCategorySt::IDENTICAL: {
                            fbSignal->m_factor = 1;
                            fbSignal->m_offset = 0;
                            break;
                        }
                        case FibexTypes::HOCompuCategorySt::TEXTTABLE: {
                            fbSignal->m_factor = 1;
                            fbSignal->m_offset = 0;
                            if (cm->m_compuInternalToPhys
                                    && (cm->m_compuInternalToPhys->m_compuScales.count() > 0)) {
                                foreach (HOCompuScale *cs, cm->m_compuInternalToPhys->m_compuScales) {
                                    if (cs->m_lowerLimit) {
                                        fbSignal->m_valueDescriptions.insert(*cs->m_lowerLimit, cs->m_compuConst.m_vt);
                                    }
                                }
                            }
                            break;
                        }
                        default:
                            break;
                    }
                }
            }
        }

    }

    {
        QHash<QString, FXPduTypeCt*> &pdus = m_elements->m_pdus;
        QHash<QString, FXPduTypeCt*>::const_iterator ci;
        for (ci = pdus.constBegin(); ci != pdus.constEnd(); ci++) {
            FBPdu *fbPdu = new FBPdu();
            const FXPduTypeCt *fxPdu = ci.value();

            fbPdu->m_fxPdu = fxPdu;

            if (fxPdu->m_sigInstances
                    && fxPdu->m_sigInstances->m_sigInstances.count() > 0) {
                QHash<QString, FXSignalInstanceType*> &sigInstances =
                        fxPdu->m_sigInstances->m_sigInstances;
                QHash<QString, FXSignalInstanceType*>::const_iterator ciSig;
                for (ciSig = sigInstances.constBegin();
                     ciSig != sigInstances.constEnd(); ciSig++) {
                    FXSignalInstanceType *si = ciSig.value();
                    if (m_signals.contains(si->m_sigRef.m_idRef)) {
                        FBSignal *fbSignal = m_signals[si->m_sigRef.m_idRef];
                        fbSignal->m_isHighLowByteOrder = si->m_commLayoutDetails.m_isBigEndian;
                        fbSignal->m_bitPosition = si->m_commLayoutDetails.m_bitPostion;
                        fbSignal->setStartBitByPosition(si->m_commLayoutDetails.m_bitPostion);
                        fbSignal->m_signalUpdateBitPosition = si->m_signalUpdateBitPosition;

                        fbPdu->m_signals[si->m_sigRef.m_idRef] = fbSignal;
                        fbSignal->m_fbPdu = fbPdu;
                    }

                }
            }

            m_pdus[fxPdu->m_id] = fbPdu;
        }
    }

    {
        QHash<QString, FXFrameTypeCt*> &frames = m_elements->m_frames;
        QHash<QString, FXFrameTypeCt*>::const_iterator ci;
        for (ci = frames.constBegin(); ci != frames.constEnd(); ci++) {
            FBFrame *fbFrame = new FBFrame();
            const FXFrameTypeCt *fxFrame = ci.value();

            fbFrame->m_fxFrame = fxFrame;

            if (fxFrame->m_pduInstances
                    && fxFrame->m_pduInstances->m_pduInstances.count() > 0) {
                QHash<QString, FXPduInstanceType*> &instances =
                        fxFrame->m_pduInstances->m_pduInstances;
                QHash<QString, FXPduInstanceType*>::const_iterator ciPdu;
                for (ciPdu = instances.constBegin();
                     ciPdu != instances.constEnd(); ciPdu++) {
                    FXPduInstanceType *ist = ciPdu.value();
                    if (m_pdus.contains(ist->m_idRef)) {
                        FBPdu *fbPdu = m_pdus[ist->m_idRef];
                        fbPdu->m_startBit = ist->m_commLayoutDetails.m_bitPostion;
                        fbPdu->m_isBigEndian = ist->m_commLayoutDetails.m_isBigEndian;

                        fbFrame->m_pdus[ist->m_idRef] = fbPdu;
                        fbFrame->m_signals.unite(fbPdu->m_signals);
                        fbPdu->m_fbFrame = fbFrame;
                    }
                }
            }

            m_frames[fxFrame->m_id] = fbFrame;
        }
    }

    {
        QHash<QString, FRChannelType*> &channels = m_elements->m_channels;
        QHash<QString, FRChannelType*>::const_iterator ci;
        for (ci = channels.constBegin(); ci != channels.constEnd(); ci++) {
            FBChannel *fbChannel = new FBChannel();
            const FRChannelType *frChannel = ci.value();

            fbChannel->m_frChannel = frChannel;

            foreach (const FXFrameTriggeringType *t, frChannel->m_frameTriggerings) {
                FBFrameTriggering *fbTriggering = new FBFrameTriggering();
                fbChannel->m_frameTriggerings[t->m_id] = fbTriggering;

                FBFrame *fbFrame = nullptr;
                if (m_frames.contains(t->m_frameRef)) {
                    fbFrame = m_frames.value(t->m_frameRef);
                }
                if (!fbFrame) { // we will update the frame which has been found
                    continue;
                }
                fbTriggering->m_frames[t->m_frameRef] = fbFrame;

                if (t->m_timings) {
                    foreach (FXAbsolutelyScheduledTimingType *rst, t->m_timings->m_absolutelyScheduledTimings) {
                        // @TODO If there are more than 1 timing, we should new frames except the first one
                        fbFrame->m_slotId = rst->m_slotId;
                        fbFrame->m_baseCycle = rst->m_baseCycle;
                        fbFrame->m_cycleRepetition = rst->m_cycleRepetition;
                        fbFrame->m_cycleCounter = rst->m_cycleCounter;
                        fbFrame->m_cycleTime = m_clusters.values().at(0)->m_msPerCycle
                                * static_cast<quint32>(fbFrame->m_cycleRepetition);
                        fbFrame->m_isTimeTriggered = true;

                        FibexFrameKey key(fbFrame->m_slotId, fbFrame->m_baseCycle, fbFrame->m_cycleRepetition);
                        fbChannel->m_frames[key] = fbFrame;
                        fbChannel->m_slotFrames.insert(fbFrame->m_slotId, fbFrame);

                        break;
                    }
                }
            }

            foreach (const FXPduTriggeringType *t, frChannel->m_pduTriggerings) {
                FBPduTriggering *fbTriggering = new FBPduTriggering();
                fbChannel->m_pduTriggerings[t->m_id] = fbTriggering;
                if (m_pdus.contains(t->m_pduRef)) {
                    fbTriggering->m_pdus[t->m_pduRef] = m_pdus.value(t->m_pduRef);
                }
            }

            m_channels[frChannel->m_id] = fbChannel;
        }
    }

    {
        QHash<QString, FXEcuType*> &ecus = m_elements->m_ecus;
        QHash<QString, FXEcuType*>::const_iterator ci;
        for (ci = ecus.constBegin(); ci != ecus.constEnd(); ci++) {
            FBEcu *fbEcu = new FBEcu();
            const FXEcuType *fxEcu = ci.value();

            m_ecus[fxEcu->m_id] = fbEcu;
            fbEcu->m_fxEcu = fxEcu;

            foreach (const FRConnectorType *connector, fxEcu->m_connectors) {
                FBChannel *fbChannel;
                if (m_channels.contains(connector->m_channelRef)) {
                    fbChannel = m_channels.value(connector->m_channelRef);
                }
                if (!fbChannel) continue;

                fbChannel->m_ecus[fxEcu->m_id] = fbEcu;

                foreach (const FXEcuPortType *port, connector->m_inputPorts) {
                    if (fbChannel->m_frameTriggerings.contains(port->m_frameTriggeringRef)) {
                        FBFrameTriggering *tr = fbChannel->m_frameTriggerings.value(port->m_frameTriggeringRef);
                        fbEcu->m_inputFrames.unite(tr->m_frames);
                    }

                    foreach (FXIncludedPduType *pdu, port->m_includedPdus) {
                        if (fbChannel->m_pduTriggerings.contains(pdu->m_pduTriggeringRef)) {
                            FBPduTriggering *tr = fbChannel->m_pduTriggerings.value(pdu->m_pduTriggeringRef);
                            fbEcu->m_inputPdus.unite(tr->m_pdus);
                        }

                    }
                }

                foreach (const FXEcuPortType *port, connector->m_outputPorts) {
                    if (fbChannel->m_frameTriggerings.contains(port->m_frameTriggeringRef)) {
                        FBFrameTriggering *tr = fbChannel->m_frameTriggerings.value(port->m_frameTriggeringRef);
                        fbEcu->m_outputFrames.unite(tr->m_frames);
                        for (auto frame : tr->m_frames)
                            frame->transmitter = fbEcu;
                    }
                }
            } 
        }
    }

    do {
        if (!m_requirements)
            break;
        QHash<QString, FXSignalGroupType*> &signalGroups = m_requirements->m_signalGroups;
        for (auto ci = signalGroups.constBegin(); ci != signalGroups.constEnd(); ci++) {
            FBSignalGroup *fbSignalGroup = new FBSignalGroup();
            const FXSignalGroupType *fxSignalGroup = ci.value();
            m_signalGroups[fxSignalGroup->m_id] = fbSignalGroup;
            fbSignalGroup->m_fxSignalGroup = fxSignalGroup;
            fbSignalGroup->m_bitLength = (fxSignalGroup->m_bitLength)?*fxSignalGroup->m_bitLength:0;
            for (const auto si : fxSignalGroup->m_orderedSignals) {
                if (m_signals.contains(si->m_sigRef.m_idRef)) {
                    FBSignal *fbSignal = m_signals[si->m_sigRef.m_idRef];
                    fbSignal->m_isGrpHighLowByteOrder = si->m_commLayoutDetails.m_isBigEndian;
                    fbSignal->m_grpBitPosition =
                            FBSignal::getStartBitByPosition(si->m_commLayoutDetails.m_bitPostion,
                                                            fbSignal->m_isGrpHighLowByteOrder,
                                                            fbSignal->m_bitSize);
                    fbSignalGroup->m_signals[si->m_sigRef.m_idRef] = fbSignal;
                    fbSignal->m_fbSignalGroup = fbSignalGroup;
                    if (!fbSignalGroup->m_fbPdu) {
                        fbSignalGroup->m_fbPdu = fbSignal->m_fbPdu;
                    }
                }
            }
        }

        auto end = m_signalGroups.end();
        for (auto it = m_signalGroups.begin(); it != end; ++it) {
            auto *fbSignalGroup = it.value();
            QString ubSignalName = fbSignalGroup->name() + "_UB";
            const auto *fbSignal = m_signalNameMap.value(ubSignalName);
            if (fbSignal) {
                fbSignalGroup->m_updateBitPosition = new quint32();
                *fbSignalGroup->m_updateBitPosition = fbSignal->m_bitPosition;
            }

            //qDebug() << *fbSignalGroup;
        }
    } while(0);

    {
        int count = 0;
#if 0
        foreach (auto fbSignal, m_signals) {
            QLOG_DEBUG() << *fbSignal;
            count++;
        }
        QLOG_INFO() << "Fibex::covertXml2Db signal count =" << count;


        count = 0;
        foreach (const FBPdu *fbPdu, m_pdus) {
            QLOG_DEBUG() << *fbPdu;
        }
        QLOG_INFO() << "Fibex::covertXml2Db pdu count =" << count;


        count = 0;
        foreach (const FBFrame *fbFrame, m_frames) {
            QLOG_DEBUG() << *fbFrame;
            count++;
        }

        QLOG_INFO() << "Fibex::covertXml2Db frame count =" << count;

#endif // if 0

#if 0
        count = 0;
        foreach (const FBChannel *fbChannel, m_channels) {
            QLOG_DEBUG() << *fbChannel;

            foreach (const FBFrame *fbFrame, fbChannel->m_frames) {
                QLOG_DEBUG() << *fbFrame;

                foreach (const FBPdu *fbPdu, fbFrame->m_pdus) {
                    QLOG_DEBUG() << *fbPdu;
                }

                foreach (const FBSignal *fbSignal, fbFrame->m_signals) {
                    QLOG_DEBUG() << *fbSignal;
                }
            }

            foreach (const FBEcu *fbEcu, fbChannel->m_ecus) {
                QLOG_DEBUG() << *fbEcu;

                QLOG_DEBUG() << "ecu" << fbEcu->name() << "input frames:" << fbEcu->m_inputFrames.count();
                foreach (auto fbFrame, fbEcu->m_inputFrames) {
                    QLOG_DEBUG() << *fbFrame;
                }

                QLOG_DEBUG() << "ecu" << fbEcu->name() << "output frames:" << fbEcu->m_outputFrames.count();
                foreach (auto fbFrame, fbEcu->m_outputFrames) {
                    QLOG_DEBUG() << *fbFrame;
                }
            }

            count++;
        }

        QLOG_INFO() << "Fibex::covertXml2Db channel count =" << count;
#endif
    }

    return true;
}

bool Fibex::createSigByUpdatebit()
{

    for (auto pdu = m_pdus.begin();pdu != m_pdus.end(); pdu++) {
        // m_instances
        if (pdu.value() == nullptr) continue;
        if (pdu.value()->m_fxPdu == nullptr) continue;

        FXSignalInstances *sigInstances = pdu.value()->m_fxPdu->m_sigInstances;
        if (sigInstances == nullptr) continue;
        auto end = sigInstances->m_sigInstances.end();

        for (auto finstance = sigInstances->m_sigInstances.begin(); finstance != end; finstance++) {
            // get updatebit
            quint32 *updateBit = finstance.value()->m_signalUpdateBitPosition;

            if (updateBit == nullptr) {
                continue;
            }

            // get sig name
            QString name;
            FXSignalRef &sigRef = finstance.value()->m_sigRef;
            if (m_signals.contains(sigRef.m_idRef)) {
                FBSignal *sig = m_signals[sigRef.m_idRef];
                if (sig == nullptr) continue;
                name = m_signals[sigRef.m_idRef]->name();
            } else {
                continue;
            }
            name += "_UB";

            FBSignal *newFbSig = new FBSignal();
            newFbSig->m_bitPosition = *updateBit;
            newFbSig->m_bitSize = 1;
            FBSignal *funSig = m_signals[sigRef.m_idRef];
            if (funSig == nullptr) continue;
            if (funSig->m_fxSignal == nullptr) continue;
            if (funSig->m_fxSignal->m_fibex == nullptr) continue;

            FXSignalType * fx = new FXSignalType(m_signals[sigRef.m_idRef]->m_fxSignal->m_fibex);
            fx->m_defaultValue = new double();
            *fx->m_defaultValue = 0;
            fx->m_shortName = name;
            newFbSig->m_fxSignal = fx;

            newFbSig->m_fbPdu = pdu.value();
            newFbSig->m_isGrpHighLowByteOrder = false;
            newFbSig->m_isHighLowByteOrder = true;
            newFbSig->m_unit="Unitless";
            newFbSig->m_maximumPhysicalValue = 1;
            newFbSig->m_factor=1;
            newFbSig->m_offset=0;
            newFbSig->m_grpBitPosition = FBSignal::getStartBitByPosition(*updateBit,
                                                                       newFbSig->m_isGrpHighLowByteOrder, 1);

            newFbSig->setStartBitByPosition(*updateBit);  // this casused bug
            newFbSig->m_codedType = CodedType::A_UINT8;
            QString idStr = sigRef.m_idRef+"_UBID";
            m_signals[idStr] = newFbSig;
            newFbSig->m_fbPdu = pdu.value();
            newFbSig->m_fbPdu->m_signals[idStr] = newFbSig;
            newFbSig->m_fbPdu->m_fbFrame->m_signals[idStr] = newFbSig;;
        }
    }

    return true;
        // update bit
        // name -> sig ref



//        FBPdu *fbPdu=const_cast<FBPdu *>(pdu.value());
//        for (auto sig = fbPdu->m_signals.begin(); sig != fbPdu->m_signals.end(); sig++) {
//            const FBSignal *fbSig = sig.value();
//            if (fbSig->m_signalUpdateBitPosition) {
//                quint32 updateBit = *(fbSig->m_signalUpdateBitPosition);
//                bool isCreated = true;
//                for (auto sig2 : fbPdu->m_signals) {
//                    if (sig2->m_bitPosition == updateBit) {
//                        isCreated=false;
//                        break;
//                    }
//                }
//                if (isCreated) {
//                    qDebug() << __FUNCTION__ << __LINE__
//                             << fbPdu->name() << " " << fbSig->name() << " pos:" << *fbSig->m_signalUpdateBitPosition << *fbSig->m_fxSignal->m_defaultValue;
//                    FBSignal *newFbSig = new FBSignal();
//                    newFbSig->m_bitPosition = updateBit;
//                    newFbSig->m_bitSize = 1;
//                    newFbSig->m_fxSignal = new FXSignalType(m_fibex);
//                    FXSignalType *m_fxSignal = const_cast <FXSignalType *>(newFbSig->m_fxSignal);
//                    m_fxSignal->m_defaultValue = 0;
//                    newFbSig->m_fbPdu = fbPdu;
//                    newFbSig->m_isUBSignal = true;
//                    newFbSig->m_isGrpHighLowByteOrder = false;
//                    newFbSig->m_isHighLowByteOrder = true;
//                    newFbSig->m_unit="Unitless";
//                    newFbSig->m_maximumPhysicalValue=1;
//                    newFbSig->m_factor=1;
//                    newFbSig->m_offset=0;
//                    newFbSig->m_grpBitPosition = FBSignal::getStartBitByPosition(updateBit,
//                                                                               newFbSig->m_isGrpHighLowByteOrder, 1);

//                    newFbSig->setStartBitByPosition(updateBit);  // this casused bug
//                    newFbSig->m_codedType = CodedType::A_UINT8;
//                    QString idStr = sig.key()+"_UBID";
//                    m_signals[idStr] = newFbSig;
//                }
//            }
//        }
//    }
//    for (auto sig = m_signals.begin(); sig != m_signals.end(); sig++)
//    {
//        if (sig.key().contains("_UBID"))
//        {
//            FBSignal *fbSig = const_cast<FBSignal *>(sig.value());
//            fbSig->m_fbPdu->m_signals[sig.key()] = fbSig;
//            fbSig->m_fbPdu->m_fbFrame->m_signals[sig.key()] = fbSig;
//        }
//    }

//    return true;
}

const FBCluster *Fibex::getFirstCluster()
{
    if (!m_clusters.count()) {
        return nullptr;
    }
    return m_clusters.values().at(0);
}

const FBChannel *Fibex::getChannelByName(const QString &channelName)
{
    Q_UNUSED(channelName)

    if (!m_channels.count()) {
        return nullptr;
    }
    return m_channels.values().at(0);
}

const FBEcu *Fibex::getNodeByName(const QString &nodeName, const QString &channelName)
{
    Q_UNUSED(channelName)

    if (!m_channels.count()) {
        return nullptr;
    }

    const FBChannel *channel = m_channels.values().at(0);
    for (auto ecu : channel->m_ecus) {
        if (ecu->name() == nodeName) {
            return ecu;
        }
    }

    return nullptr;
}

bool Fibex::load(const QDomElement &element)
{   
    bool ret;

    /* load Fibex tree */
    ret = FXFibex::load(element);
    if (!ret) {
        return false;
    }

    if(!covertXml2Db()) {
            return false;
    }
    return createSigByUpdatebit();
}

} // FIBEX
} // ASAM
