#include "FXIncludedSignalType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXIncludedSignalType::FXIncludedSignalType(FXFibex *fibex)
    : m_fibex(fibex)
{
}

void FXIncludedSignalType::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXIncludedSignalType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:SIGNAL-INSTANCE-REF") {
            m_signalInstanceRef = childElement.attribute("ID-REF");
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "FXIncludedSignalType::load, m_signalInstanceRef =" << m_signalInstanceRef;
#endif
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
