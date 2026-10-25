#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType/element SIGNAL-REF
 *
 * Refering a signal
 */
class ASAM_FIBEX_EXPORT FXSignalRef
{
public:
    FXSignalRef();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** attribute ID-REF */
    QString m_idRef;
};

} // FIBEX
} // ASAM
