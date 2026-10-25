#pragma once

#include "platform.h"
#include "vector_dbc_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief element LONG-NAME
 *
 * The elements full name in the language given as attribute
 */
class VECTOR_DBC_EXPORT HOLongName
{
public:
    HOLongName();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @brief LONG-NAME */
    QString m_longName;
};

}
}

