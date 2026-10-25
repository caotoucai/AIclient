#include "HOUnitSpec.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOUnitSpec::HOUnitSpec(FXFibex *fibex)
    : m_fibex(fibex)
{
}

HOUnitSpec::~HOUnitSpec()
{
    qDeleteAll(m_units);
    m_units.clear();
}

void HOUnitSpec::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXProcessingInformation::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:ADMIN-DATA") {

        } else if (childElement.tagName() == "ho:PHYSICAL-DIMENSIONS") {

        } else if (childElement.tagName() == "ho:UNITGROUPS") {

        } else if (childElement.tagName() == "ho:UNITS") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "HOUnitSpec::load UNITS" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "ho:UNIT") {
                    HOUnit *unit = new HOUnit(m_fibex);
                    unit->load(subChild.toElement());
                    m_units[unit->m_id] = unit;
                }
                subChild = subChild.nextSibling();
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
