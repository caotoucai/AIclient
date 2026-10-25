#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "HONameDetails.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType NAMED-ELEMENT-TYPE
 *
 * Base content model for all ELEMENTS
 */
class ASAM_FIBEX_EXPORT FXNamedElementType : public HONameDetails
{
public:
    FXNamedElementType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @must attribute ID */
    QString m_id;

    /** @todo attribute OID */
};

}
}
