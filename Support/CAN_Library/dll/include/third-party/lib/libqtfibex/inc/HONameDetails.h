#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief group NAME-DETAILS
 *
 * Common naming attributes
 */
class ASAM_FIBEX_EXPORT HONameDetails
{
public:
    HONameDetails();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    HOShortName m_shortName;
    HOLongName m_longName;
    HODesc m_desc;
};

}
}

