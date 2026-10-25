#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXNamedElementType.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType REVISED-ELEMENT-TYPE
 *
 * Base content model for all ELEMENTS
 */
class ASAM_FIBEX_EXPORT FXRevisedElementType : public FXNamedElementType
{
public:
    FXRevisedElementType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element ELEMENT-REVISIONS */
    /** @todo element PRODUCT-REF */
};

}
}

