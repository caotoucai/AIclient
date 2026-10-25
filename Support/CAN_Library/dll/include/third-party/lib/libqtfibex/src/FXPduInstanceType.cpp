#include "FXPduInstanceType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXPduInstanceType::FXPduInstanceType(FXFibex *fibex)
    : FXGenericPduInstanceType()
    , m_fibex(fibex)
{
}

void FXPduInstanceType::load(const QDomElement &element)
{
    FXGenericPduInstanceType::load(element);
    m_commLayoutDetails.load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXPduInstanceType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:PDU-UPDATE-BIT-POSITION") {
            if (!m_pduUpdateBitPosition) {
                m_pduUpdateBitPosition = new quint32();
                *m_pduUpdateBitPosition = childElement.text().toUInt();
            }
        }
        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
