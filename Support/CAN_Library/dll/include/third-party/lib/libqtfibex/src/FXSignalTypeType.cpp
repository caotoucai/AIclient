#include "../inc/FXSignalTypeType.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXSignalTypeType::FXSignalTypeType()
{
}

FXSignalTypeType::~FXSignalTypeType()
{
    if (m_method) delete m_method;
    m_attributes.clear();
}

void FXSignalTypeType::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "FXSignalTypeType::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "fx:TYPE") {
            QString typeStr = childElement.text();
            FibexTypes::EnumParser<FibexTypes::FXTypeTypeSt> ep;
            FibexTypes::FXTypeTypeSt type;
            bool isOk = ep.str2Enum(typeStr, type);
            if (isOk) {
                m_type = type;
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXSignalTypeType::load, m_type =" << typeStr << (int)m_type;
#endif
            }
        } else if (childElement.tagName() == "fx:METHOD") {
            if (!m_method) {
                m_method = new QString();
                *m_method = childElement.text();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXSignalTypeType::load, m_method =" << *m_method;
#endif
            }

        } else if (childElement.tagName() == "fx:ATTRIBUTES") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
                const QDomElement &subChildElement = subChild.toElement();
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "FXSignalTypeType::load ATTRIBUTES" << subChildElement.tagName();
#endif
                if (subChildElement.tagName() == "fx:ATTRIBUTE") {
                    QString attr = subChildElement.text();
                    m_attributes.append(attr);
                }
                subChild = subChild.nextSibling();
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
