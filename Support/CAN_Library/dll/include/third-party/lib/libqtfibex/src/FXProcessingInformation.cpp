#include "FXProcessingInformation.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXProcessingInformation::FXProcessingInformation(FXFibex *fibex)
    : m_fibex(fibex)
{
}

FXProcessingInformation::~FXProcessingInformation()
{
    if (m_unitSpec) delete m_unitSpec;
    if (m_codings) delete m_codings;
}

void FXProcessingInformation::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXProcessingInformation::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:UNIT-SPEC") {
            if (!m_unitSpec) {
                m_unitSpec = new HOUnitSpec(m_fibex);
                m_unitSpec->load(childElement);
            }
        } else if (childElement.tagName() == "fx:CODINGS") {
            if (!m_codings) {
                m_codings = new FXCodings(m_fibex);
                m_codings->load(childElement);
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
