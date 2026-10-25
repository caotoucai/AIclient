#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType TIME-RANGE-TYPE
 *
 * For reusability time ranges content model is specified using a type definition.
 */
class ASAM_FIBEX_EXPORT FXTimeRangeType
{
public:
    FXTimeRangeType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element VALUE */
    QString m_value;
    /** @todo element TOLERANCE */
};

} // FIBEX
} // ASAM
