#include "FXCommonLayoutDetails.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXCommonLayoutDetails::FXCommonLayoutDetails()
{
}

void FXCommonLayoutDetails::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXCommonLayoutDetails::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:BIT-POSITION") {
            m_bitPostion = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXCommonLayoutDetails::load, m_bitPostion =" << m_bitPostion;
#endif
        } else if (childElement.tagName() == "fx:IS-HIGH-LOW-BYTE-ORDER") {
            m_isBigEndian = QVariant(childElement.text()).toBool();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXCommonLayoutDetails::load, m_isBigEndian =" << m_isBigEndian;
#endif
        } else if (childElement.tagName() == "fx:SEQUENCE-NUMBER") {
            m_sequenceNumber = childElement.text().toUShort();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXCommonLayoutDetails::load, m_sequenceNumber =" << m_sequenceNumber;
#endif
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
