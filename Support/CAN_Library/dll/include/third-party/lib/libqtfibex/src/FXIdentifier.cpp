#include "FXIdentifier.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXIdentifier::FXIdentifier()
{
}

void FXIdentifier::load(const QDomElement &element)
{
    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXRelativelyScheduledTimingType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:IDENTIFIER-VALUE") {
            m_identifierValue = childElement.text().toLong();
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
