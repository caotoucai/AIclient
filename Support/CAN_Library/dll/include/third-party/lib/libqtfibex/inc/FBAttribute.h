
#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FBAttributeValueType.h"

namespace ASAM {
namespace FIBEX {

class ASAM_FIBEX_EXPORT FBAttribute
{
public:
    FBAttribute();

    /** Name */
    QString name;

    /** Value Type */
    FBAttributeValueType valueType;

    union {
        /** Integer Value of type AttributeValueType::Int */
        int integerValue;

        /** Hex Value of type AttributeValueType::Hex */
        int hexValue;

        /** Float Value of type AttributeValueType::Float */
        double floatValue;

        /** Enum Value of type AttributeValueType::Enum (used only for BA enums) */
        int enumValue;

        // std::string doesn't work in a union, so it's below
    };

    /** String Value of type AttributeValueType::String (used only for BA_DEF_DEF enums) */
    QString stringValue;
};

}
}
