#include "FXIncludedPduType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXIncludedPduType::FXIncludedPduType(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXIncludedPduType::~FXIncludedPduType()
{
    qDeleteAll(m_includedSignals);
    m_includedSignals.clear();
}

void FXIncludedPduType::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXIncludedPduType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:PDU-TRIGGERING-REF") {
            m_pduTriggeringRef = childElement.attribute("ID-REF");
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXIncludedPduType::load, m_pduTriggeringRef =" << m_pduTriggeringRef;
#endif
        } else if (childElement.tagName() == "fx:INCLUDED-SIGNALS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXIncludedPduType::load INCLUDED-SIGNALS" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:INCLUDED-SIGNAL") {
                    FXIncludedSignalType *sig = new FXIncludedSignalType(m_fibex);
                    sig->load(subChildElement);
                    m_includedSignals.append(sig);
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:COMPLETE-PDU") {
            m_completePdu = true;
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
