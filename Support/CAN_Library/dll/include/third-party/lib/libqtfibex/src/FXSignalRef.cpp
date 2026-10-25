#include "FXSignalRef.h"
#include "LogDb.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

FXSignalRef::FXSignalRef()
{
}

void FXSignalRef::load(const QDomElement &element)
{
    m_idRef = element.attribute("ID-REF");
#ifndef F_NO_DEBUG
    QLOG_TRACE() << "FXSignalRef::load, m_idRef =" << m_idRef;
#endif
}

}
}
