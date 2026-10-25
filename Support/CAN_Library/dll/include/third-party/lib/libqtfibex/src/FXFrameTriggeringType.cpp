#include "FXFrameTriggeringType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXFrameTriggeringType::FXFrameTriggeringType(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXFrameTriggeringType::~FXFrameTriggeringType()
{
    if (m_timings) delete m_timings;
}

void FXFrameTriggeringType::load(const QDomElement &element)
{
    m_id = element.attribute("ID");
#ifndef F_NO_DEBUG
    QLOG_TRACE() << "FrameTriggeringType::load, m_id =" << m_id;
#endif

    /* read childs */
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FrameTriggeringType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:TIMINGS") {
            if (!m_timings) {
                m_timings = new FXFrameTimings(m_fibex);
                m_timings->load(childElement);
            }
        } else if (childElement.tagName() == "fx:IDENTIFIER") {
            m_identifier.load(childElement);
        } else if (childElement.tagName() == "fx:FRAME-REF") {
            m_frameRef = childElement.attribute("ID-REF");
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
