#include "FXFrameTimings.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXFrameTimings::FXFrameTimings(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXFrameTimings::~FXFrameTimings()
{
    qDeleteAll(m_relativelyScheduledTimings);
    m_relativelyScheduledTimings.clear();

    qDeleteAll(m_absolutelyScheduledTimings);
    m_absolutelyScheduledTimings.clear();
}

void FXFrameTimings::load(const QDomElement &element)
{
    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXFrameTimings::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:RELATIVELY-SCHEDULED-TIMING") {
            FXRelativelyScheduledTimingType *relativelyScheduledTiming = new FXRelativelyScheduledTimingType(m_fibex);
            relativelyScheduledTiming->load(childElement);
            m_relativelyScheduledTimings.append(relativelyScheduledTiming);
        } else if (childElement.tagName() == "fx:ABSOLUTELY-SCHEDULED-TIMING") {
            FXAbsolutelyScheduledTimingType *absolutelyScheduledTiming = new FXAbsolutelyScheduledTimingType(m_fibex);
            absolutelyScheduledTiming->load(childElement);
            m_absolutelyScheduledTimings.append(absolutelyScheduledTiming);
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
