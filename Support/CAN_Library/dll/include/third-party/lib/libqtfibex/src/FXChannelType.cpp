#include "FXChannelType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXChannelType::FXChannelType(FXFibex *fibex)
    : FXRevisedElementType()
    , m_fibex(fibex)
{
}

FXChannelType::~FXChannelType()
{
    qDeleteAll(m_pduTriggerings);
    m_pduTriggerings.clear();

    qDeleteAll(m_frameTriggerings);
    m_frameTriggerings.clear();
}

void FXChannelType::load(const QDomElement &element)
{
    FXRevisedElementType::load(element);

    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXChannelType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:PDU-TRIGGERINGS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXChannelType::load fx:PDU-TRIGGERINGS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:PDU-TRIGGERING") {
                    FXPduTriggeringType *tr = new FXPduTriggeringType(m_fibex);
                    tr->load(subChildElement);
                    m_pduTriggerings[tr->m_id] = tr;
                }
                subChild = subChild.nextSibling();
            }

        } else if (childElement.tagName() == "fx:FRAME-TRIGGERINGS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXChannelType::load fx:FRAME-TRIGGERINGS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:FRAME-TRIGGERING") {
                    FXFrameTriggeringType *tr = new FXFrameTriggeringType(m_fibex);
                    tr->load(subChildElement);
                    m_frameTriggerings[tr->m_id] = tr;
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
