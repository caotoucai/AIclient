#include "HOCompuRationalCoeffs.h"
#include "LogDb.h"
#include "FXFibex.h"

#include <QDomElement>

namespace ASAM {
namespace FIBEX {

HOCompuRationalCoeffs::HOCompuRationalCoeffs(FXFibex *fibex)
    : m_fibex(fibex)
{
}

void HOCompuRationalCoeffs::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
#ifndef F_NO_DEBUG
        QLOG_TRACE() << "HOCompuRationalCoeffs::load" << childElement.tagName();
#endif
        if (childElement.tagName() == "ho:COMPU-NUMERATOR") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "HOCompuRationalCoeffs::load COMPU-NUMERATOR" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "ho:V") {
                    double v = subChild.toElement().text().toDouble();
                    m_compuNumeratorList.append(v);
                }
                subChild = subChild.nextSibling();
            }
        } else if (childElement.tagName() == "ho:COMPU-DENOMINATOR") {
            QDomNode subChild = child.firstChild();
            while (!subChild.isNull()) {
#ifndef F_NO_DEBUG
                QLOG_TRACE() << "HOCompuRationalCoeffs::load COMPU-DENOMINATOR" << childElement.tagName();
#endif
                if (subChild.toElement().tagName() == "ho:V") {
                    double v = subChild.toElement().text().toDouble();
                    m_compuDenominatorList.append(v);
                }
                subChild = subChild.nextSibling();
            }
        }

        child = child.nextSibling();
    }
}

} // FIBEX
} // ASAM
