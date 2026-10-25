#include "../inc/FXElements.h"
#include "../inc/LogDb.h"
#include "../inc/FXFibex.h"

#include <QDomElement>
#include <QDebug>

namespace ASAM {
namespace FIBEX {

FXElements::FXElements(FXFibex *fibex)
    : m_fibex(fibex)
    , m_clusters()
    , m_channels()
    , m_ecus()
    , m_pdus()
    , m_frames()
    , m_signals()
    , m_gateways()
    , m_functions()
    , m_composites()
{
}

FXElements::~FXElements()
{
    qDeleteAll(m_clusters);
    m_clusters.clear();
    qDeleteAll(m_channels);
    m_channels.clear();
    qDeleteAll(m_gateways);
    m_gateways.clear();
    qDeleteAll(m_ecus);
    m_ecus.clear();
    qDeleteAll(m_pdus);
    m_pdus.clear();
    qDeleteAll(m_frames);
    m_frames.clear();
    qDeleteAll(m_signals);
    m_signals.clear();
    qDeleteAll(m_functions);
    m_functions.clear();
    qDeleteAll(m_composites);
    m_composites.clear();

}

bool FXElements::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXElements::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:SIGNALS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXElements::load SIGNALS" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "fx:SIGNAL") {
                    FXSignalType *sig = new FXSignalType(m_fibex);
                    sig->load(subChild.toElement());
                    if (!sig->m_id.isEmpty()) {
                        m_signals[sig->m_id] = sig;
                    }
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:PDUS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXElements::load PDUS" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "fx:PDU") {
                    FXPduTypeCt *pdu = new FXPduTypeCt(m_fibex);
                    pdu->load(subChild.toElement());
                    if (!pdu->m_id.isEmpty()) {
                        m_pdus[pdu->m_id] = pdu;
                    }
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:FRAMES") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXElements::load FRAMES" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "fx:FRAME") {
                    FXFrameTypeCt *frame = new FXFrameTypeCt(m_fibex);
                    frame->load(subChild.toElement());
                    if (!frame->m_id.isEmpty()) {
                        m_frames[frame->m_id] = frame;
                    }
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:ECUS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXElements::load ECUS" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "fx:ECU") {
                    FXEcuType *ecu = new FXEcuType(m_fibex);
                    ecu->load(subChild.toElement());
                    if (!ecu->m_id.isEmpty()) {
                        m_ecus[ecu->m_id] = ecu;
                    }
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:CHANNELS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXElements::load CHANNELS" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "fx:CHANNEL") {
                    FRChannelType *c = new FRChannelType(m_fibex);
                    c->load(subChild.toElement());
                    m_channels[c->m_id] = c;
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:CLUSTERS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXElements::load CLUSTERS" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "fx:CLUSTER") {
                    FRClusterType *c = new FRClusterType(m_fibex);
                    c->load(subChild.toElement());
                    m_clusters[c->m_id] = c;
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:GATEWAYS") {

        } else if (childElement.tagName() == "fx:FUNCTIONS") {

        } else if (childElement.tagName() == "fx:COMPOSITES") {

        }

        child = child.nextSibling();
    }

    return true;
}

} // FIBEX
} // ASAM
