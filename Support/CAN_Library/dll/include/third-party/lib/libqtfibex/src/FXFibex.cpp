#include "../inc/FXFibex.h"
#include "../inc/LogDb.h"

#include <QDomElement>
#include <QDebug>

namespace ASAM {
namespace FIBEX {

FXFibex::FXFibex()
{
}

FXFibex::~FXFibex()
{
    if (m_elements) delete m_elements;
    if (m_processingInformation) delete m_processingInformation;
    if (m_requirements) delete m_requirements;
}

bool FXFibex::load(const QDomElement &element)
{
    bool ret = true;

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXFibex::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:PROCESSING-INFORMATION") {
            if (!m_processingInformation) {
                m_processingInformation = new FXProcessingInformation(this);
                m_processingInformation->load(childElement);
            }
        } else if (childElement.tagName() == "fx:ELEMENTS") {
            if (!m_elements) {
                m_elements = new FXElements(this);
                m_elements->load(child.toElement());
            }
        } else if (childElement.tagName() == "fx:PROJECT") {

        } else if (childElement.tagName() == "fx:PROTOCOLS") {

        } else if (childElement.tagName() == "fx:REQUIREMENTS") {
            if (!m_requirements) {
                m_requirements = new FXRequirements(this);
                m_requirements->load(child.toElement());
            }
        }

        child = child.nextSibling();
    }

    return ret;
}

}
}
