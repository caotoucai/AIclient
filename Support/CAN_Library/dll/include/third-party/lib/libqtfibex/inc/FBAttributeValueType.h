#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

namespace ASAM {
namespace FIBEX {

/**
 * Attribute Value Type
 */
enum class FBAttributeValueType {
    Int,        /**< Integer */
    Hex,        /**< Hex */
    Float,      /**< Float */
    String,     /**< String */
    Enum        /**< Enum */
};

}
}
