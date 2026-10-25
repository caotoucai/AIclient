#include "HOCompuMethod.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOCompuMethod::HOCompuMethod(FXFibex *fibex)
    : HONameDetails()
    , m_fibex(fibex)
{
}

HOCompuMethod::~HOCompuMethod()
{
    if (m_unitRef) delete m_unitRef;
    if (m_physConstrs) delete m_physConstrs;
    if (m_internalConstrs) delete m_internalConstrs;
    if (m_compuInternalToPhys) delete m_compuInternalToPhys;
}

void HOCompuMethod::load(const QDomElement &element)
{
    HONameDetails::load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOCompuMethod::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:CATEGORY") {
            QString typeStr = childElement.text();
            FibexTypes::EnumParser<FibexTypes::HOCompuCategorySt> ep;
            FibexTypes::HOCompuCategorySt type;
            bool isOk = ep.str2Enum(typeStr, type);
            if (isOk) {
                m_compuCategory = type;
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "HOCompuMethod::load, m_compuCategory =" << typeStr << (int)m_compuCategory;
#endif
            }
        } else if (childElement.tagName() == "ho:UNIT-REF") {
            if (!m_unitRef) {
                m_unitRef = new QString();
                *m_unitRef = childElement.attribute("ID-REF");
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "HOCompuMethod::load, m_unitRef =" << *m_unitRef;
#endif
            }

        } else if (childElement.tagName() == "ho:PHYS-CONSTRS") {
            if (!m_physConstrs) {
                m_physConstrs = new HOScaleConstrType(m_fibex);
                m_physConstrs->load(childElement);
            }
        } else if (childElement.tagName() == "ho:INTERNAL-CONSTRS") {
            if (!m_internalConstrs) {
                m_internalConstrs = new HOScaleConstrType(m_fibex);
                m_internalConstrs->load(childElement);
            }
        } else if (childElement.tagName() == "ho:COMPU-INTERNAL-TO-PHYS") {
            if (!m_compuInternalToPhys) {
                m_compuInternalToPhys = new HOCompInternalToPhys(m_fibex);
                m_compuInternalToPhys->load(childElement);
            }
        } else if (childElement.tagName() == "ho:DESC") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
