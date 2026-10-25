#include "HOLongName.h"

#include <QDomElement>
#include <QDebug>

namespace ASAM {
namespace FIBEX {

HOLongName::HOLongName()
    : m_longName()
{
}

void HOLongName::load(const QDomElement &element)
{
    QDomNode child = element.firstChild();
    while (!child.isNull()) {
        const QDomElement &childElement = child.toElement();
        qDebug() << "FrameType::load" << childElement.tagName();

        if (childElement.tagName() == "ho:LONG-NAME") {
            m_longName = childElement.text();
            qDebug() << "LongName::load, m_longName =" << m_longName;
        }

        child = child.nextSibling();
    }
}

}
}
