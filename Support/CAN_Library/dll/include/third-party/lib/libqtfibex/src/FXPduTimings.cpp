#include "FXPduTimings.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXPduTimings::FXPduTimings(FXFibex *fibex)
    : m_fibex(fibex)
{
}

void FXPduTimings::load(const QDomElement &element)
{
    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXPduTimings::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:CYCLIC-TIMING") {
            m_cyclicTiming.load(childElement);
        } else if (childElement.tagName() == "fx:EVENT-CONTROLLED-TIMING") {
            m_eventControlledTiming.load(childElement);
        } else if (childElement.tagName() == "fx:REQUEST-CONTROLLED-TIMING") {
            m_requestControlledTiming.load(childElement);
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
