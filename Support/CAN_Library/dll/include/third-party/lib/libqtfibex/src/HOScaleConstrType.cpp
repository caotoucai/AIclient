#include "HOScaleConstrType.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOScaleConstrType::HOScaleConstrType(FXFibex *fibex)
    : m_fibex(fibex)
    , m_lowerLimitIntervalType(FibexTypes::HOIntervalTypeAb::CLOSED)
    , m_upperLimitIntervalType(FibexTypes::HOIntervalTypeAb::CLOSED)
{
}

HOScaleConstrType::~HOScaleConstrType()
{
    if (m_lowerLimit) delete m_lowerLimit;
    if (m_upperLimit) delete m_upperLimit;
    if (m_scaleConstr) delete m_scaleConstr;
}

void HOScaleConstrType::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOScaleConstrType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:LOWER-LIMIT") {
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
                    QLOG_TRACE() << "HOScaleConstrType::load, m_lowerLimitIntervalType =" << typeStr << (int)m_lowerLimitIntervalType;
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
                    QLOG_TRACE() << "HOScaleConstrType::load, m_upperLimitIntervalType =" << typeStr << (int)m_upperLimitIntervalType;
    #endif
                }
            }

        } else if (childElement.tagName() == "ho:SCALE-CONSTR") {
            if (!m_scaleConstr) {
                m_scaleConstr = new HOScaleConstr(m_fibex);
                m_scaleConstr->load(childElement);
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
