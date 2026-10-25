#include "HOUnit.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOUnit::HOUnit(FXFibex *fibex)
    : HONameDetails()
    , m_fibex(fibex)
{
}

HOUnit::~HOUnit()
{
    if (m_displayName) delete m_displayName;
    if (m_factorSiToUnit) delete m_factorSiToUnit;
    if (m_offsetSiToUnit) delete m_offsetSiToUnit;
    if (m_physicalDimensionRef) delete m_physicalDimensionRef;
}

void HOUnit::load(const QDomElement &element)
{
    m_id = element.attribute("ID");
#ifndef F_NO_DEBUG
    QLOG_TRACE() << "HOUnit::load, m_id =" << m_id;
#endif

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOUnit::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:DISPLAY-NAME") {
            if (!m_displayName) {
                m_displayName = new QString();
                *m_displayName = childElement.text();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "HOUnit::load, m_displayName =" << *m_displayName;
#endif
            }

        } else if (childElement.tagName() == "ho:FACTOR-SI-TO-UNIT") {
            if (!m_factorSiToUnit) {
                m_factorSiToUnit = new double();
                *m_factorSiToUnit = childElement.text().toDouble();
            }
        } else if (childElement.tagName() == "ho:OFFSET-SI-TO-UNIT") {
            if (!m_offsetSiToUnit) {
                m_offsetSiToUnit = new double();
                *m_offsetSiToUnit = childElement.text().toDouble();
            }
        } else if (childElement.tagName() == "ho:PHYSICAL-DIMENSION-REF") {
            if (!m_physicalDimensionRef) {
                m_physicalDimensionRef = new QString();
                *m_physicalDimensionRef = childElement.attribute("ID-REF");
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
