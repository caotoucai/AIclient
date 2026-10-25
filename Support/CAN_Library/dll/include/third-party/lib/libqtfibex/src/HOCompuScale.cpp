#include "HOCompuScale.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOCompuScale::HOCompuScale(FXFibex *fibex)
    : m_fibex(fibex)
    , m_lowerLimitIntervalType(FibexTypes::HOIntervalTypeAb::CLOSED)
    , m_upperLimitIntervalType(FibexTypes::HOIntervalTypeAb::CLOSED)
{
}

HOCompuScale::~HOCompuScale()
{
    if (m_lowerLimit) delete m_lowerLimit;
    if (m_upperLimit) delete m_upperLimit;
    if (m_compuRationalCoeffs) delete m_compuRationalCoeffs;
}

void HOCompuScale::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOCompuScale::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:SHORT-LABEL") {

        } else if (childElement.tagName() == "ho:DESC") {

        } else if (childElement.tagName() == "ho:MASK") {

        } else if (childElement.tagName() == "ho:LOWER-LIMIT") {
            if (!m_lowerLimit) {
                m_lowerLimit = new double();
                *m_lowerLimit = childElement.text().toDouble();

                QString typeStr = childElement.attribute("INTERVAL-TYPE");
                FibexTypes::EnumParser<FibexTypes::HOIntervalTypeAb> ep;
                FibexTypes::HOIntervalTypeAb type;
                bool isOk = ep.str2Enum(typeStr, type);
                if (isOk) {
                    m_lowerLimitIntervalType = type;
#ifndef F_NO_DEBUG
                    QLOG_TRACE() << "HOCompuScale::load, m_lowerLimitIntervalType =" << typeStr << (int)m_lowerLimitIntervalType;
#endif
                }
            }
        } else if (childElement.tagName() == "ho:UPPER-LIMIT") {
            if (!m_upperLimit) {
                m_upperLimit = new double();
                *m_upperLimit = childElement.text().toDouble();

                QString typeStr = childElement.attribute("INTERVAL-TYPE");
                FibexTypes::EnumParser<FibexTypes::HOIntervalTypeAb> ep;
                FibexTypes::HOIntervalTypeAb type;
                bool isOk = ep.str2Enum(typeStr, type);
                if (isOk) {
                    m_upperLimitIntervalType = type;
#ifndef F_NO_DEBUG
                    QLOG_TRACE() << "HOCompuScale::load, m_upperLimitIntervalType =" << typeStr << (int)m_upperLimitIntervalType;
#endif
                }
            }
        } else if (childElement.tagName() == "ho:COMPU-CONST") {
            m_compuConst.load(childElement);
        } else if (childElement.tagName() == "ho:COMPU-RATIONAL-COEFFS") {
            if (!m_compuRationalCoeffs) {
                m_compuRationalCoeffs = new HOCompuRationalCoeffs(m_fibex);
                m_compuRationalCoeffs->load(childElement);
            }
        } else if (childElement.tagName() == "ho:COMPU-GENERIC-MATH") {

        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
