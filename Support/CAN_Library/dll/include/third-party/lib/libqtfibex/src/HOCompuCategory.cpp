#include "HOCompuCategory.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOCompuCategory::HOCompuCategory()
{
}

void HOCompuCategory::load(const QDomElement &element)
{
    QString typeStr = element.text();
    FibexTypes::EnumParser<FibexTypes::HOCompuCategorySt> ep;
    FibexTypes::HOCompuCategorySt type;
    bool isOk = ep.str2Enum(typeStr, type);
    if (isOk) {
        m_compuCategory = type;
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "CompuCategory::load, m_compuCategory =" << typeStr << (int)m_compuCategory;
#endif
    }
}

} // FIBEX
} // ASAM
