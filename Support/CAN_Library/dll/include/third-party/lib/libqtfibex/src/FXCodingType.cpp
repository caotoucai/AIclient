#include "FXCodingType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

CodingType::CodingType(FXFibex *fibex)
    : FXRevisedElementType()
    , m_fibex(fibex)
{
}

CodingType::~CodingType()
{
    if (m_codedType) delete m_codedType;
    if (m_compuMethods) delete m_compuMethods;
}

void CodingType::load(const QDomElement &element)
{
    FXRevisedElementType::load(element);

    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "CodingType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:PHYSICAL-TYPE") {

        } else if (childElement.tagName() == "ho:CODED-TYPE") {
            if (!m_codedType) {
                m_codedType = new HOCodedType(m_fibex);
                m_codedType->load(childElement);
            }
        } else if (childElement.tagName() == "ho:COMPU-METHODS") {
            if (!m_compuMethods) {
                m_compuMethods = new HOCompuMethods(m_fibex);
                m_compuMethods->load(childElement);
            }
        } else if (childElement.tagName() == "fx:MANUFACTURER-EXTENSION") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
