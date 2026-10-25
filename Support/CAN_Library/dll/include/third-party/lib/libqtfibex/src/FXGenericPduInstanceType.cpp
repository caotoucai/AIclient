#include "FXGenericPduInstanceType.h"
#include "LogDb.h"

#include <QDomElement>
#include <QDebug>

namespace ASAM {
namespace FIBEX {

FXGenericPduInstanceType::FXGenericPduInstanceType()
{
}

void FXGenericPduInstanceType::load(const QDomElement &element)
{
    m_id = element.attribute("ID");
#ifndef F_NO_DEBUG
    QLOG_TRACE() << "FXGenericPduInstanceType::load, m_id =" << m_id;
#endif

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXGenericPduInstanceType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:PDU-REF") {
            m_idRef = childElement.attribute("ID-REF");
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXGenericPduInstanceType::load, m_idRef =" << m_idRef;
#endif
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

}
}
