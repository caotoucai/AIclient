#include "HONameDetails.h"
#include "LogDb.h"

#include <QDomElement>
#include <QDebug>

namespace ASAM {
namespace FIBEX {

HONameDetails::HONameDetails()
    : m_shortName()
    , m_longName()
    , m_desc()
{
}

void HONameDetails::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HONameDetails::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:LONG-NAME") {
            m_longName = childElement.text().simplified();;
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "HONameDetails::load, m_longName =" << m_longName;
#endif
        } else if (childElement.tagName() == "ho:SHORT-NAME") {
            m_shortName = childElement.text().simplified();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "HONameDetails::load, m_shortName =" << m_shortName;
#endif
        } else if (childElement.tagName() == "ho:DESC") {
            m_desc = childElement.text().simplified();
#ifndef F_NO_DEBUG
            QLOG_TRACE() << "HONameDetails::load, m_desc =" << m_desc;
#endif
        }

        child = child.nextSibling();
    }
}

}
}
