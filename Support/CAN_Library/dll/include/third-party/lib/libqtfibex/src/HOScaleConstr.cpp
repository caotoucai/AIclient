#include "HOScaleConstr.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOScaleConstr::HOScaleConstr(FXFibex *fibex)
    : m_fibex(fibex)
    , m_lowerLimitIntervalType(FibexTypes::HOIntervalTypeAb::CLOSED)
    , m_upperLimitIntervalType(FibexTypes::HOIntervalTypeAb::CLOSED)
{
}

HOScaleConstr::~HOScaleConstr()
{
    if (m_lowerLimit) delete m_lowerLimit;
    if (m_upperLimit) delete m_upperLimit;
}

void HOScaleConstr::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOScaleConstr::load" << childElement.tagName();
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
                    QLOG_TRACE() << "HOScaleConstr::load, m_lowerLimitIntervalType =" << typeStr << (int)m_lowerLimitIntervalType;
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
                    QLOG_TRACE() << "HOScaleConstr::load, m_upperLimitIntervalType =" << typeStr << (int)m_upperLimitIntervalType;
    #endif
                }
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
