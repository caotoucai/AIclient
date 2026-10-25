#include "FXEcuPortType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXEcuPortType::FXEcuPortType(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXEcuPortType::~FXEcuPortType()
{
    qDeleteAll(m_includedPdus);
    m_includedPdus.clear();
}

void FXEcuPortType::load(const QDomElement &element)
{
    m_id = element.attribute("ID");

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXEcuPortType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:FRAME-TRIGGERING-REF") {
            m_frameTriggeringRef = childElement.attribute("ID-REF");
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXEcuPortType::load, m_frameTriggeringRef =" << m_frameTriggeringRef;
#endif
        } else if (childElement.tagName() == "fx:INCLUDED-PDUS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXEcuPortType::load INCLUDED-PDU" << childElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:INCLUDED-PDU") {
                    FXIncludedPduType *pdu = new FXIncludedPduType(m_fibex);
                    pdu->load(subChildElement);
                    m_includedPdus.append(pdu);
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "fx:COMPLETE-FRAME") {
            m_completeFrame = true;
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
