#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType SIGNAL-TYPE-TYPE
 *
 * General category (type) of a SIGNAL
 */
class ASAM_FIBEX_EXPORT FXSignalTypeType
{
public:
    FXSignalTypeType();
    ~FXSignalTypeType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @required element TYPE */
    FibexTypes::FXTypeTypeSt m_type;
    /** @opt element METHOD */
    QString *m_method = nullptr;

    /** @todo element ATTRIBUTES */
    QList<QString> m_attributes;
};

} // FIBEX
} // ASAM
